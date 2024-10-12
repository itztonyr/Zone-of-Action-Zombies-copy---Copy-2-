<?php
require_once('bl_Common.php');
const AUTH_DB_NAME = 'authTokens';

if (isset($_POST['type'])) {
  $type  = $_POST['type'];

  if ($type == 0) {
    httpRequest();
  } else if ($type == 1) {
    checkAuthPassport('fbSessionsdb', 'fbSessions');
  } else if ($type == 2) {
    checkAuthPassport('gSessionsdb', 'gSessions');
  } else if ($type == 3) {
    $db_name   = $_POST['dbname'];
    checkAuthPassport($db_name);
  } else if ($type == 4) {
    echo "<script>poptastic($url);</script>";
  }
} else if (isset($_GET['code']) && isset($_GET['state'])) {
  authRedirect();
} else {
  http_response_code(400);
}

/*
* Make HTTP request
*/
function httpRequest()
{
  $url = $_POST['url'];

  $ch = curl_init();
  curl_setopt($ch, CURLOPT_USERAGENT, "Mozilla/5.0 (Windows; U; Windows NT 5.1; rv:1.7.3) Gecko/20041001 Firefox/0.10.1");
  curl_setopt($ch, CURLOPT_POST, 1);
  curl_setopt($ch, CURLOPT_URL, $url);
  curl_setopt($ch, CURLOPT_FOLLOWLOCATION, true);
  curl_setopt($ch, CURLOPT_RETURNTRANSFER, true);
  curl_setopt($ch, CURLOPT_HTTPHEADER, array(
    'Content-Type: application/x-www-form-urlencoded',
    'Content-Length: 0'
  ));
  curl_setopt($ch, CURLOPT_CONNECTTIMEOUT, 0);
  curl_setopt($ch, CURLOPT_TIMEOUT, 400);
  curl_setopt($ch, CURLOPT_MAXREDIRS, 10);
  $content = curl_exec($ch);
  echo $content;
}

/*
* Check if an authentication token exists in the database
*/
function checkAuthPassport($dbName, $fileName = '')
{
  $fileName = $fileName ?: $dbName;

  $state = $_POST['state'];

  $db = Utils::get_sqlite_db("{$fileName}.db", $dbName);
  if (!$db) {
    die("Unable to access the database.");
  }

  $result = $db->query("SELECT * FROM $dbName WHERE state = '$state'");

  if ($result) {
    $found = false;
    while ($row = $result->fetchArray()) {
      if (!isset($row['state'])) {
        $db->close();
        die("not found");
      }
      echo "success|{$row['state']}|{$row['code']}";
      $db->query("DELETE FROM $dbName WHERE state = '$state'");
      $found = true;
    }

    if (!$found) {
      echo "not found";
    }
  } else {
    $db->lastErrorMsg();
  }
  $db->close();
}

/*
* OAuth Redirect callback handler
*/
function authRedirect()
{
  $state = $_GET['state'];
  $code  = $_GET['code'];

  // Validate input data
  if (!isset($state, $code) || empty($state) || empty($code)) {
    die('Invalid input data');
  }

  // Use prepared statements to prevent SQL injection
  $db = Utils::get_sqlite_db(AUTH_DB_NAME . '.db', AUTH_DB_NAME);
  $stmt = $db->prepare("INSERT INTO " . AUTH_DB_NAME . " (state, code) VALUES (:state, :code)");
  $stmt->bindParam(':state', $state);
  $stmt->bindParam(':code', $code);
  $result = $stmt->execute();

  // Handle result
  if ($result) {
    $content = '<p class="center-text">Close this window and back to the game.</p>
        <button class="button-6">Continue</button>';
  } else {
    $content = '<p class="center-text">' . $db->lastErrorMsg() . '</p>';
  }

  // Use a template engine or a separate HTML file for output
  $htmlTemplate = file_get_contents('templates/oauth-redirect.htm');
  $htmlContent = str_replace(
    "#CONTENT#",
    $content,
    $htmlTemplate
  );

  echo $htmlContent;

  $db->close();
}
