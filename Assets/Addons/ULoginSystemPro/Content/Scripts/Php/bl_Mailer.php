<?php

require_once('bl_Common.php');
require __DIR__ . '/vendor/autoload.php';

use PHPMailer\PHPMailer\PHPMailer;
use PHPMailer\PHPMailer\Exception;

const IS_SMTP = true;
const SMTP_HOST = "lovattocloud.com";
const SMTP_USER = "no-reply@lovattocloud.com";
const SMTP_PASSWORD = "Mrlovo071234!";
const SMTP_PORT = 587; // Normally, 465 for SSL, 587 for TLS
const USE_TTLS = true;

class MailCreator
{
    public function Send($from, $to, $subject, $message)
    {
        if (IS_SMTP) {
            $mail = new PHPMailer(true);
            try {
                $mail->IsSMTP();
                $mail->SMTPAuth = true;
                $mail->CharSet     = "UTF-8";
                $mail->Debugoutput = 'html';
                $mail->Host        = SMTP_HOST;
                $mail->Port        = SMTP_PORT;
                if (USE_TTLS == false) {
                    $mail->SMTPSecure = PHPMailer::ENCRYPTION_SMTPS;
                } else {
                    $mail->SMTPSecure = PHPMailer::ENCRYPTION_STARTTLS;
                }
                $mail->IsHTML(true);
                $mail->Username = SMTP_USER;
                $mail->Password = SMTP_PASSWORD;
                $mail->From     = $from;
                $mail->FromName = GAME_NAME;
                $mail->AddAddress($to);
                $mail->AddReplyTo($from, GAME_NAME);
                $mail->Subject = $subject;
                $mail->AltBody = "To view the message, please use an HTML compatible email viewer!";
                $mail->Body    = $message;
                if (!$mail->Send()) {
                    echo "Message could not be sent. Mailer Error: {$mail->ErrorInfo}";
                    return false;
                } else {
                    return true;
                }
            } catch (Exception $e) {
                echo "Message could not be sent. Mailer Error: {$mail->ErrorInfo}";
                return false;
            }
        } else {
            $headers = "From:" . $from . "\r\n";
            $headers .= "Reply-To: " . $from . "\r\n";
            $headers .= "MIME-Version: 1.0\r\n";
            $headers .= "Content-Type: text/html; charset=ISO-8859-1\r\n";

            if (mail($to, $subject, $message, $headers)) {
                return true;
            } else {
                return false;
            }
        }
    }
}
