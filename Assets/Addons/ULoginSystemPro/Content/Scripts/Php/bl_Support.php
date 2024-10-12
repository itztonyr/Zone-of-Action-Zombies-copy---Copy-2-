<?php

if (!isset($_POST['type']) || !isset($_POST['hash'])) {
    http_response_code(400);
    exit();
}

include("bl_Common.php");
const TABLE_NAME = "bl_game_tickets";

$hash = Utils::sanitaze_var($_POST['hash']);
$real_hash = Utils::get_secret_hash('');
if ($real_hash != $hash) {
    http_response_code(401);
    exit();
}

$link = Connection::dbConnect();

$ticket = new Tickets($link);
$type = Utils::sanitaze_var($_POST['type']);

if ($type == 1) submitTicket();
else if ($type == 2) checkTicketForUser();
else if ($type == 3) getAllTickets();
else if ($type == 4) staffReply();
else if ($type == 5) updateTicketStatus();
else if ($type == 6) deleteTicket();

$link->close();

function submitTicket()
{
    global $ticket;
    global $link;

    $userId = Utils::sanitaze_var($_POST['userId'], $link);
    $title = Utils::sanitaze_var($_POST['title'], $link);
    $chat = Utils::sanitaze_var($_POST['chat'], $link);

    $result = $ticket->createTicket($userId, $title, $chat, 1);
    if ($result) {
        http_response_code(202);
    }
}

function checkTicketForUser()
{
    global $ticket;
    global $link;

    $userId = Utils::sanitaze_var($_POST['userId'], $link);
    $result = $ticket->getTicketById($userId);
    if ($result) {
        http_response_code(201);

        $response = array(
            "id" => $result['id'],
            "user_id" => $result['user_id'],
            "title" => $result['title'],
            "chat" => $result['chat'],
            "status" => $result['status'],
            "last_update" => $result['last_update'],
        );
        echo json_encode($response);
    } else {
        http_response_code(204);
    }
}

function getAllTickets()
{
    global $ticket;
    $result = $ticket->getTickets();
    if ($result->num_rows > 0) {
        http_response_code(200);
        $response = array();
        $response["tickets"] = array();
        while ($row = $result->fetch_assoc()) {
            $response["tickets"][] = array(
                "id" => $row['id'],
                "user_id" => $row['user_id'],
                "title" => $row['title'],
                "chat" => $row['chat'],
                "status" => $row['status'],
                "last_update" => $row['last_update'],
            );
        }

        echo json_encode($response);
    } else {
        http_response_code(204);
    }
}

function staffReply()
{
    global $ticket;
    global $link;

    $id = Utils::sanitaze_var($_POST['id'], $link);
    $reply = Utils::sanitaze_var($_POST['reply'], $link);
    $userId = Utils::sanitaze_var($_POST['userId'], $link);
    $userNick = Utils::sanitaze_var($_POST['userNick'], $link);
    $isUser = isset($_POST['isUser']);

    $result = $ticket->addReplyToTicket($id, $reply, $userId, $userNick, $isUser);
    if ($result) {
        http_response_code(202);
    }
}

function deleteTicket()
{
    global $ticket;
    global $link;

    $id = Utils::sanitaze_var($_POST['id'], $link);
    $result = $ticket->deleteTicket($id);
    if ($result) {
        http_response_code(202);
    }
}

function updateTicketStatus()
{
    global $ticket;
    global $link;

    $id = Utils::sanitaze_var($_POST['id'], $link);
    $status = Utils::sanitaze_var($_POST['status'], $link);
    $result = $ticket->updateTicketStatus($id, $status);
    if ($result) {
        http_response_code(202);
    }
}

class Tickets
{
    private $conn;
    public function __construct($con)
    {
        $this->conn = $con;
    }

    public function getTickets()
    {
        $sql = "SELECT * FROM " . TABLE_NAME;
        $result = $this->conn->query($sql);
        return $result;
    }

    public function getTicket($id)
    {
        $sql = "SELECT * FROM " . TABLE_NAME . " WHERE id = $id";
        $result = $this->conn->query($sql);
        // check if the query returned any results
        if ($result->num_rows > 0) {
            // get result as an associative array
            $ticket = $result->fetch_assoc();
            return $ticket;
        }

        return false;
    }

    public function getTicketById($id)
    {
        // get the first open or pending ticket from the user id
        $sql = "SELECT * FROM " . TABLE_NAME . " WHERE user_id = $id AND status = 1 OR status = 3 ORDER BY id DESC LIMIT 1";
        $result = $this->conn->query($sql);
        // check if the result is not empty
        if ($result->num_rows > 0) {
            // return the first ticket
            return $result->fetch_assoc();
        }
        // return null if no ticket found
        return false;
    }

    public function createTicket($userId, $title, $chat, $status)
    {
        $sql = "INSERT INTO " . TABLE_NAME . " (user_id, title, chat, status) VALUES ($userId, '$title', '$chat', '$status')";
        $result = $this->conn->query($sql) or die(mysqli_error($this->conn));
        return $result;
    }

    public function updateTicket($id, $title, $description, $status, $updated_at)
    {
        $sql = "UPDATE " . TABLE_NAME . " SET title = '$title', description = '$description', status = '$status', updated_at = '$updated_at' WHERE id = $id";
        $result = $this->conn->query($sql);
        return $result;
    }

    public function updateTicketStatus($id, $status)
    {
        $sql = "UPDATE " . TABLE_NAME . " SET status = '$status' WHERE id = $id";
        $result = $this->conn->query($sql);
        return $result;
    }

    /**
     * Adds a reply to a ticket from the staff.
     *
     * @param int $ticketId The ID of the ticket.
     * @param string $reply The reply message.
     * @param int $userId The ID of the staff user.
     * @param string $userNick The nickname of the staff user.
     * @return bool Returns true if the reply was added successfully, false otherwise.
     */
    public function addReplyToTicket($ticketId, $reply, $userId, $userNick, $fromUser)
    {
        $ticket = $this->getTicket($ticketId);
        if (!$ticket) {
            die("Ticket not found");
        }

        $nick = $userNick;
        if (!$fromUser) {
            $nick = $userNick . ' (Staff)';
        }

        $chat = $ticket['chat'];
        $json = json_decode($chat, true, 512);
        $replyData = array(
            "user_id" => (int)$userId,
            "nick" => $nick,
            "text" => $reply,
        );
        $json["chat"][] = $replyData;

        // encode json and make sure to encode in a format where multiple spaces are scaped
        $chat = json_encode($json);
        $chat = addslashes($chat);

        $newStatus = $fromUser ? 1 : 3;

        $sql = "UPDATE " . TABLE_NAME . " SET chat = '$chat', status = '$newStatus' WHERE id = $ticketId";
        $result = $this->conn->query($sql);
        // check if the query was successful
        if ($result) {
            return true;
        }
        return false;
    }

    public function deleteTicket($id)
    {
        $sql = "DELETE FROM " . TABLE_NAME . " WHERE id = $id";
        $result = $this->conn->query($sql);
        return $result;
    }
}
