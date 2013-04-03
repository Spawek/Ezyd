-- phpMyAdmin SQL Dump
-- version 3.3.9
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Czas wygenerowania: 26 Lut 2012, 21:27
-- Wersja serwera: 5.5.8
-- Wersja PHP: 5.3.5

SET SQL_MODE="NO_AUTO_VALUE_ON_ZERO";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8 */;

--
-- Baza danych: `ezyd`
--

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `friends`
--

CREATE TABLE IF NOT EXISTS `friends` (
  `senderID` bigint(20) unsigned NOT NULL,
  `receiverID` bigint(20) unsigned NOT NULL,
  `receiverAccepted` tinyint(4) NOT NULL,
  KEY `senderID` (`senderID`),
  KEY `receiverID` (`receiverID`),
  KEY `receiverAccepted` (`receiverAccepted`),
  KEY `senderID_2` (`senderID`,`receiverAccepted`),
  KEY `receiverID_2` (`receiverID`,`receiverAccepted`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `friends`
--

INSERT INTO `friends` (`senderID`, `receiverID`, `receiverAccepted`) VALUES
(100000907961641, 123312321, 1),
(100000907961641, 100000488478449, 1),
(100000907961641, 100000381030141, 1),
(100000488478449, 100000381030141, 1),
(100000560899331, 1364901534, 0),
(100000560899331, 100000907961641, 1),
(100000907961641, 100000464503530, 1),
(100000907961641, 100000054258013, 1),
(100000464503530, 100000054258013, 1);

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_accepted_history`
--

CREATE TABLE IF NOT EXISTS `transactions_accepted_history` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext CHARACTER SET latin2 NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text CHARACTER SET latin2 NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_accepted_history`
--

INSERT INTO `transactions_accepted_history` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(7, 100000907961641, 666, 'PLN', '2012-02-17 00:00:28', 1, ' '),
(7, 100000488478449, -333, 'PLN', '2012-02-17 00:00:28', 1, ' '),
(7, 100000381030141, -333, 'PLN', '2012-02-17 00:00:28', 1, ' '),
(7, 0, 0, 'PLN', '2012-02-17 00:00:28', 1, 'optimization test'),
(14, 100000907961641, 66, 'PLN', '2012-02-17 00:42:22', 1, ' '),
(14, 100000381030141, -33, 'PLN', '2012-02-17 00:42:22', 1, ' '),
(14, 100000488478449, -33, 'PLN', '2012-02-17 00:42:22', 1, ' '),
(14, 0, 0, 'PLN', '2012-02-17 00:42:22', 1, ''),
(14, 100000907961641, 66, 'PLN', '2012-02-17 00:44:01', 1, ' '),
(14, 100000381030141, -33, 'PLN', '2012-02-17 00:44:01', 1, ' '),
(14, 100000488478449, -33, 'PLN', '2012-02-17 00:44:01', 1, ' '),
(14, 0, 0, 'PLN', '2012-02-17 00:44:01', 1, ''),
(14, 100000907961641, 66, 'PLN', '2012-02-17 00:49:03', 1, ' '),
(14, 100000381030141, -33, 'PLN', '2012-02-17 00:49:03', 1, ' '),
(14, 100000488478449, -33, 'PLN', '2012-02-17 00:49:03', 1, ' '),
(14, 0, 0, 'PLN', '2012-02-17 00:49:03', 1, ''),
(26, 100000907961641, -18000, 'PLN', '2012-02-20 15:32:28', 1, ' '),
(26, 100000054258013, 8000, 'PLN', '2012-02-20 15:32:28', 1, ' '),
(26, 100000464503530, 10000, 'PLN', '2012-02-20 15:32:28', 1, ' '),
(26, 0, 0, 'PLN', '2012-02-20 15:32:28', 1, 'test2'),
(31, 100000907961641, 10000, 'PLN', '2012-02-20 15:47:43', 1, ' '),
(31, 100000464503530, -10000, 'PLN', '2012-02-20 15:47:43', 1, ' '),
(31, 0, 0, 'PLN', '2012-02-20 15:47:43', 1, ''),
(36, 100000907961641, 1332, 'PLN', '2012-02-20 16:12:00', 1, ' '),
(36, 100000054258013, -666, 'PLN', '2012-02-20 16:12:00', 1, ' '),
(36, 100000464503530, -666, 'PLN', '2012-02-20 16:12:00', 1, ' '),
(36, 0, 0, 'PLN', '2012-02-20 16:12:00', 1, ''),
(46, 100000907961641, 6150, 'EUR', '2012-02-26 22:10:04', 1, ' '),
(46, 100000054258013, -6150, 'EUR', '2012-02-26 22:10:04', 1, ' '),
(46, 0, 0, 'EUR', '2012-02-26 22:10:04', 1, 'płacimy w euro'),
(45, 100000907961641, 22650, 'PLN', '2012-02-26 22:10:36', 1, 'ąłżóśńćź'),
(45, 100000054258013, -22650, 'PLN', '2012-02-26 22:10:36', 1, ' '),
(45, 0, 0, 'PLN', '2012-02-26 22:10:36', 1, ''),
(44, 100000907961641, -26700, 'PLN', '2012-02-26 22:11:23', 1, 'alzczns'),
(44, 100000054258013, 26700, 'PLN', '2012-02-26 22:11:23', 1, ' '),
(44, 0, 0, 'PLN', '2012-02-26 22:11:23', 1, '');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_cancelled`
--

CREATE TABLE IF NOT EXISTS `transactions_cancelled` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext CHARACTER SET latin2 NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text CHARACTER SET latin2 NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_cancelled`
--

INSERT INTO `transactions_cancelled` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(6, 100000907961641, -33, 'PLN', '2012-02-20 15:50:29', 0, ' '),
(6, 100001444831695, 66, 'PLN', '2012-02-20 15:50:29', 0, ' '),
(6, 100002264130468, -33, 'PLN', '2012-02-20 15:50:29', 0, ' '),
(6, 0, 0, 'PLN', '2012-02-20 15:50:29', 1, ''),
(35, 100000907961641, 2600, 'PLN', '2012-02-20 16:04:24', 0, 'testMSG'),
(35, 100000464503530, -2600, 'PLN', '2012-02-20 16:04:24', 0, ' '),
(35, 0, 0, 'PLN', '2012-02-20 16:04:24', 1, ''),
(39, 100000117638801, 1200, 'PLN', '2012-02-26 10:40:55', 0, ' '),
(39, 100000659197363, -1200, 'PLN', '2012-02-26 10:40:55', 0, ' '),
(39, 0, 0, 'PLN', '2012-02-26 10:40:55', 1, 'Pizza'),
(41, 100000004854111, 2950, 'PLN', '2012-02-26 10:41:16', 0, ' '),
(41, 100000659197363, -2950, 'PLN', '2012-02-26 10:41:16', 0, ' '),
(41, 0, 0, 'PLN', '2012-02-26 10:41:16', 1, 'liskacz z urbanem ');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_history`
--

CREATE TABLE IF NOT EXISTS `transactions_history` (
  `transactionID` int(10) unsigned NOT NULL,
  `plusGuyID` bigint(20) unsigned NOT NULL,
  `minusGuyID` bigint(20) unsigned NOT NULL,
  `value` int(10) unsigned NOT NULL,
  `currency` tinytext CHARACTER SET latin2 NOT NULL,
  `date` datetime NOT NULL,
  `fulfillDate` datetime DEFAULT NULL,
  `desc` text CHARACTER SET latin2,
  KEY `transactionID` (`transactionID`),
  KEY `plusGuyID` (`plusGuyID`),
  KEY `minusGuyID` (`minusGuyID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_history`
--

INSERT INTO `transactions_history` (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `fulfillDate`, `desc`) VALUES
(12, 100000907961641, 100000488478449, 666, 'PLN', '2012-02-17 00:26:02', '2012-02-17 00:38:53', ' '),
(13, 100000907961641, 100000381030141, 666, 'PLN', '2012-02-17 00:26:02', '2012-02-17 00:38:57', ' '),
(15, 100000907961641, 100000381030141, 33, 'PLN', '0000-00-00 00:00:00', '2012-02-20 15:37:44', ' '),
(16, 100000907961641, 100000488478449, 33, 'PLN', '0000-00-00 00:00:00', '2012-02-20 15:45:26', ' '),
(33, 100000907961641, 100000464503530, 2000, 'PLN', '0000-00-00 00:00:00', '2012-02-20 15:49:30', ' '),
(17, 100000907961641, 100000381030141, 33, 'PLN', '0000-00-00 00:00:00', '2012-02-20 16:09:34', NULL);

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_pending`
--

CREATE TABLE IF NOT EXISTS `transactions_pending` (
  `transactionID` int(10) unsigned NOT NULL,
  `plusGuyID` bigint(20) unsigned NOT NULL,
  `minusGuyID` bigint(20) unsigned NOT NULL,
  `value` int(10) unsigned NOT NULL,
  `currency` tinytext CHARACTER SET latin2,
  `date` datetime DEFAULT NULL,
  `desc` text CHARACTER SET latin2,
  KEY `plusGuyID` (`plusGuyID`),
  KEY `minusGuyID` (`minusGuyID`),
  KEY `date` (`date`),
  KEY `transactionID` (`transactionID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_pending`
--

INSERT INTO `transactions_pending` (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) VALUES
(18, 100000907961641, 100000488478449, 33, 'PLN', '2012-02-17 00:44:01', ' '),
(19, 100000907961641, 100000381030141, 33, 'PLN', '2012-02-17 00:49:03', ' '),
(20, 100000907961641, 100000488478449, 33, 'PLN', '2012-02-17 00:49:03', ' '),
(36, 100000907961641, 100000464503530, 1332, 'PLN', '2012-02-20 16:12:00', ' '),
(37, 100000054258013, 100000464503530, 3334, 'PLN', '2012-02-20 16:12:00', ' '),
(46, 100000907961641, 100000054258013, 6150, 'EUR', '2012-02-26 22:10:04', ' '),
(47, 100000054258013, 100000907961641, 4050, 'PLN', '2012-02-26 22:11:23', ' ');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_reqs`
--

CREATE TABLE IF NOT EXISTS `transactions_reqs` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext CHARACTER SET latin2 NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text CHARACTER SET latin2 NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_reqs`
--

INSERT INTO `transactions_reqs` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(2, 100000907961641, 750, 'PLN', '2012-02-15 17:34:45', 0, ' '),
(2, 123312321, -750, 'PLN', '2012-02-15 17:34:45', 0, ' '),
(2, 0, 0, 'PLN', '2012-02-15 17:34:45', 1, ''),
(21, 100000907961641, -2000, 'PLN', '2012-02-20 14:07:26', 1, ' '),
(21, 100000560899331, 2000, 'PLN', '2012-02-20 14:07:26', 0, ' '),
(21, 0, 0, 'PLN', '2012-02-20 14:07:26', 1, 'warsztaty'),
(22, 100000907961641, -1000, 'PLN', '2012-02-20 14:07:54', 1, ' '),
(22, 100000465526654, 1000, 'PLN', '2012-02-20 14:07:54', 0, ' '),
(22, 0, 0, 'PLN', '2012-02-20 14:07:54', 1, 'warsztaty'),
(38, 100000907961641, 1450, 'PLN', '2012-02-25 19:49:14', 1, ' '),
(38, 100000004854111, -1450, 'PLN', '2012-02-25 19:49:14', 0, ' '),
(38, 0, 0, 'PLN', '2012-02-25 19:49:14', 1, 'Wóda z Marykla '),
(40, 100000907961641, 10000, 'PLN', '2012-02-25 21:48:26', 0, ' '),
(40, 100000004854111, -10000, 'PLN', '2012-02-25 21:48:26', 0, ' '),
(40, 0, 0, 'PLN', '2012-02-25 21:48:26', 1, 'pozyczna na liskacza (marylka)'),
(42, 100000907961641, -6150, 'PLN', '2012-02-25 23:06:51', 0, 'llll'),
(42, 100000054258013, 6150, 'PLN', '2012-02-25 23:06:51', 1, ' '),
(42, 0, 0, 'PLN', '2012-02-25 23:06:51', 1, ''),
(43, 100000907961641, 6150, 'PLN', '2012-02-26 11:56:07', 0, 'lazócze'),
(43, 100000054258013, -6150, 'PLN', '2012-02-26 11:56:07', 1, ' '),
(43, 0, 0, 'PLN', '2012-02-26 11:56:07', 1, '');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `users`
--

CREATE TABLE IF NOT EXISTS `users` (
  `ID` bigint(20) unsigned NOT NULL,
  `firstname` varchar(25) DEFAULT NULL,
  `surname` varchar(50) DEFAULT NULL,
  UNIQUE KEY `ID` (`ID`),
  KEY `firstname` (`firstname`,`surname`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `users`
--

INSERT INTO `users` (`ID`, `firstname`, `surname`) VALUES
(43, NULL, NULL),
(654, NULL, NULL),
(666, NULL, NULL),
(669, NULL, NULL),
(18979456, 'Macu', 'Piccu');
