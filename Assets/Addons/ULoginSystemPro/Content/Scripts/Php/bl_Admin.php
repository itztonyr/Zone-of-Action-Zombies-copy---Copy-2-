<?php
include_once("bl_Common.php");
include_once("bl_Functions.php");
include_once("bl_Mailer.php");

$link = Connection::dbConnect();

if (isset($_POST['sid'])) {
    $sid   = Utils::sanitaze_var($_POST['sid'], $link);
}
Utils::check_session($_POST['sid']);

if (isset($_POST['userId'])) {
    $userId  = Utils::sanitaze_var($_POST['userId'], $link, $sid);
}
if (isset($_POST['name'])) {
    $name  = Utils::sanitaze_var($_POST['name'], $link, $sid);
}
if (isset($_POST['data'])) {
    $data  = Utils::sanitaze_var($_POST['data'], $link, $sid);
}
if (isset($_POST['type'])) {
    $type  = Utils::sanitaze_var($_POST['type'], $link, $sid);
}
if (isset($_POST['ip'])) {
    $ip    = Utils::sanitaze_var($_POST['ip'], $link, $sid);
}
if (isset($_POST['deviceId'])) {
    $deviceId    = Utils::sanitaze_var($_POST['deviceId'], $link, $sid);
}
if (isset($_POST['author'])) {
    $autor = Utils::sanitaze_var($_POST['author'], $link, $sid);
}
if (isset($_POST['hash'])) {
    $hash  = Utils::sanitaze_var($_POST['hash'], $link, $sid);
}

$real_hash = Utils::get_secret_hash($name);
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$functions = new Functions($link);

switch ($type) {
    case 1: //Do a ban
        if (!isset($userId) || $userId == '') {
            die('You are not using the last version of ULogin Pro.');
        }

        if (!isset($ip) || $ip == '') {
            $sql = "SELECT ip FROM " . PLAYERS_DB . " WHERE id='$userId'";
            $result = mysqli_query($link, $sql) or die(mysqli_error($link));

            $row = mysqli_fetch_assoc($result);
            $ip = $row['ip'];
        }

        $check = mysqli_query($link, "SELECT * FROM " . BANS_DB . " WHERE `name`= '$name' OR `user_id`='$userId'");
        if (mysqli_num_rows($check) != 0) {
            die("This player is already banned.");
        }
        $query = "INSERT INTO " . BANS_DB . " (`user_id`, `name` ,  `reason` ,  `ip` ,  `by`, `device_id` ) VALUES ('$userId' , '$name' ,  '$data' ,  '$ip',  '$autor', '$deviceId');";
        $query .= "UPDATE " . PLAYERS_DB . " SET status='3' WHERE name= '$name'";

        if ($functions->multiple_query($query)) {
            echo "success";
        }
        break;
    case 2: //UnBan a player
        $sql       = "DELETE FROM " . BANS_DB . " WHERE name= '$name';";
        $sql .= "UPDATE " . PLAYERS_DB . " SET status='0' WHERE name= '$name'";

        if (mysqli_multi_query($link, $sql) or die(mysqli_error($link))) {
            echo "success";
        }
        break;
    case 3: //Change player status
        setPlayerStatus($data, $name, "name");
        break;
    case 4: //search user by their name and get their basic stats
        $result = $functions->get_user_by('name', $name);
        if (!$result) {
            $result = $functions->get_user_by('nick', $name);
            if (!$result) {
                die("User with this name or nick name does not exist in DataBase!");
            }
            echo FetchBasicStats($result);
        } else {
            echo FetchBasicStats($result);
        }
        break;
    case 5: //update player values given from the client side
        $check = mysqli_query($link, "SELECT * FROM " . PLAYERS_DB . " WHERE id='$name'") or die(mysqli_error($link));

        if (mysqli_num_rows($check) == 0) {
            die("Player " . $name . " not found.");
        }

        $values = $_POST['unsafe'];

        $query = mysqli_query($link, "UPDATE " . PLAYERS_DB . " SET " . $values . " WHERE id='$name'") or die(mysqli_error($link));
        if ($query) {
            echo "done";
        }
        break;
    case 6: //Get database stats
        $result = mysqli_query($link, "SELECT count(*) as total from " . PLAYERS_DB);
        $data = mysqli_fetch_assoc($result);
        $tablecount = $data['total'];
        $result2 = mysqli_query($link, "SELECT count(*) as total from " . BANS_DB);
        $data2 = mysqli_fetch_assoc($result2);
        $tablecount2 = $data2['total'];
        $result3 = mysqli_query($link, "SELECT SUM(playtime) as total from " . PLAYERS_DB);
        $data3 = mysqli_fetch_assoc($result3);
        $tablecount3 = $data3['total'];
        $lastp = mysqli_query($link, "SELECT nick FROM " . PLAYERS_DB . " ORDER BY `id` DESC LIMIT 1") or die(mysqli_error($link));
        $lastone = mysqli_fetch_assoc($lastp);
        echo "success|" . $tablecount . "|" . $lastone["nick"] . "|" . $tablecount2 . "|" . $tablecount3;
        break;
    case 7:
        testEmail();
        break;
    case 8:
        setPlayerStatus($data, $name, "id");
        break;
    case 9:
        getUserList();
        break;
    case 10:
        deleteAccount();
        break;
}
mysqli_close($link);

/*
* Send a test email
*/
function testEmail()
{

    global $data;

    $subject = GAME_NAME . " Email Test";
    $htmlContent = "If you receive this email it means that your email server is working correctly.";

    $mailer = new MailCreator();
    if ($mailer->Send(ADMIN_EMAIL, $data, $subject, $htmlContent)) {
        http_response_code(202);
    } else {
        http_response_code(406);
    }
    exit();
}

/*
* Change user account status
*/
function setPlayerStatus($newStatus, $userId, $where)
{
    global $link;

    $query = "UPDATE " . PLAYERS_DB . " SET status='" . $newStatus . "' WHERE " . $where . "='$userId'";
    if (mysqli_query($link, $query) or  die(mysqli_error($link))) {
        echo "success";
    }
}

/*
* Get a list of the users with pagination
*/
function getUserList()
{
    global $link;
    global $sid;

    $page = Utils::sanitaze_var($_POST['page'], $link, $sid);
    $limit = Utils::sanitaze_var($_POST['count'], $link, $sid);

    $start_from = ($page - 1) * $limit;

    $sql = "SELECT * FROM " . PLAYERS_DB . " ORDER BY id ASC LIMIT $start_from, $limit";
    $result = mysqli_query($link, $sql) or die(mysqli_error($link));

    $num_results = mysqli_num_rows($result);

    if ($num_results <= 0) {
        http_response_code(204);
        exit();
    }

    $response["users"] = array();

    while ($r = mysqli_fetch_assoc($result)) {
        unset($r['password']);
        $response["users"][] = $r;
    }

    $query = "SELECT COUNT(id) AS total_count FROM " . PLAYERS_DB;
    $result = mysqli_query($link, $query) or die(mysqli_error($link));
    $row = mysqli_fetch_assoc($result);
    $response["total"] = $row['total_count'];

    echo json_encode($response, JSON_PRETTY_PRINT);
}

/*
* Delete a player account
*/
function deleteAccount()
{
    global $link;
    global $sid;

    $id = Utils::sanitaze_var($_POST['id'], $link, $sid);

    $query = "DELETE FROM " . PLAYERS_DB . " WHERE id='$id'";
    mysqli_query($link, $query) or die(mysqli_error($link));

    http_response_code(202);
}

/*
*
*/
function FetchBasicStats($row)
{
    $stats = "";
    $stats = "success|" . $row['name'] . "|" . $row['kills'] . "|" . $row['deaths'] . "|" . $row['score'] . "|" . $row['ip'] . "|" . $row['status'] . "|" . $row['playtime'] . "|" . $row['nick'] . "|" . $row['id'] . "|"
        . $row['coins'] . "|" . $row['user_date'];
    return $stats;
}
