<?php
include_once("bl_Common.php");
require __DIR__ . '/vendor/autoload.php';

use phpseclib\Crypt\RSA;

const KEY_LENGTH = 2048;
const AES_INDICATOR = 'seae#';
const AES_METHOD = 'aes-256-cbc';
const STATIC_KEYS = true; // True for static keys, false for dynamic keys

/*
*
*/
function generateKeyPair()
{
    if (STATIC_KEYS) {
        generateStaticKeys();
        $publicKey = file_get_contents('keys/public.pem');
        return $publicKey;
    } else {
        if (!isset($_SESSION['rsapublickey'])) {
            $rsa = new RSA();
            $rsa->setPrivateKeyFormat(RSA::PRIVATE_FORMAT_XML);
            $rsa->setPublicKeyFormat(RSA::PUBLIC_FORMAT_XML);
            $keys = $rsa->createKey(KEY_LENGTH);
            $_SESSION['rsaprivatekey'] = $keys['privatekey'];
            $_SESSION['rsapublickey'] = $keys['publickey'];
        }
        return $_SESSION['rsapublickey'];
    }
}

/*
*
*/
function generateStaticKeys()
{
    if (file_exists('keys/public.pem')) {
        return;
    }

    if (!is_dir('keys')) {
        mkdir('keys', 0600, true);
    }

    $rsa = new RSA();
    $rsa->setPrivateKeyFormat(RSA::PRIVATE_FORMAT_XML);
    $rsa->setPublicKeyFormat(RSA::PUBLIC_FORMAT_XML);
    $keys = $rsa->createKey(KEY_LENGTH);
    $privateKey = $keys['privatekey'];
    $publicKey = $keys['publickey'];

    file_put_contents('keys/private.pem', $privateKey);
    file_put_contents('keys/public.pem', $publicKey);
}

/*
*
*/
function encrypt($clearText)
{
    $rsa = new RSA();
    $rsa->setEncryptionMode(RSA::ENCRYPTION_PKCS1);
    $rsa->setPublicKeyFormat(RSA::PUBLIC_FORMAT_XML);
    if (STATIC_KEYS) {
        $rsa->loadKey(file_get_contents('keys/public.pem'));
    } else {
        $rsa->loadKey($_SESSION['rsapublickey']);
    }
    $bytesCipherText = $rsa->encrypt($clearText);
    return rawurlencode(base64_encode($bytesCipherText));
}

/*
*
*/
function decrypt($encrypted)
{
    if (strlen($encrypted) > 5 && substr($encrypted, 0, strlen(AES_INDICATOR)) === AES_INDICATOR) {
        $encrypted = str_replace(substr($encrypted, 0, strlen(AES_INDICATOR)), "", $encrypted);
        $clearText = AESdecryptFromClient($encrypted);
        echo '-result(' . $clearText . '):(' . $encrypted . ')';
        return $clearText;
    }

    $rsa = new RSA();
    $rsa->setEncryptionMode(RSA::ENCRYPTION_PKCS1);
    $rsa->setPrivateKeyFormat(RSA::PRIVATE_FORMAT_XML);
    if (STATIC_KEYS) {
        $rsa->loadKey(file_get_contents('keys/private.pem'));
    } else {
        $rsa->loadKey($_SESSION['rsaprivatekey']);
    }
    $bytesCipherText = base64_decode(rawurldecode($encrypted));
    $clearText = $rsa->decrypt($bytesCipherText);
    return $clearText;
}

/*
* 
*/
function AESdecryptFromClient($encrypted)
{
    $hash = hash("sha256", SECRET_KEY, true);
    $key = substr($hash, 0, 32);
    $secretKey = base64_encode($key);
    $secretKey = substr($secretKey, 0, 32);

    $encryptedBytes = base64_decode($encrypted);
    $iv = substr($encryptedBytes, 0, 16);
    $encryptedData = substr($encryptedBytes, 16);
    $decryptedData = openssl_decrypt($encryptedData, AES_METHOD, $secretKey, OPENSSL_RAW_DATA, $iv);
    return $decryptedData;
}

/*
* 
*/
function AESdecrypt($encrypted, $sid)
{
    $iv = get_iv();
    $password = substr(hash('sha256', $sid, true), 0, 32);
    $decrypted = openssl_decrypt(base64_decode($encrypted), AES_METHOD, $password, OPENSSL_RAW_DATA, $iv);
    return $decrypted;
}

/*
* 
*/
function AESencrypt($decrypted, $sid)
{
    $iv = get_iv();
    $password = substr(hash('sha256', $sid, true), 0, 32);
    $encrypted = base64_encode(openssl_encrypt($decrypted, AES_METHOD, $password, OPENSSL_RAW_DATA, $iv));
    return $encrypted;
}

/*
* 
*/
function get_iv()
{
    return chr(0x49) . chr(0x76) . chr(0x61) . chr(0x62) . chr(0x20) . chr(0x4d) . chr(0x65) . chr(0x64) . chr(0x76) . chr(0x65) . chr(0x76) . chr(0x6e) . chr(0x65) . chr(0x6e) . chr(0x76) . chr(0x76);
}

/*
* 
*/
function CheckSession($sid)
{
    if (!STATIC_KEYS && isset($sid)) {
        session_id($sid);
        session_start();
    }
}

if (isset($_POST['keygen'])) {
    CheckSession($_POST['session_id']);
    EchoWithPrefix(generateKeyPair());
    exit();
}
