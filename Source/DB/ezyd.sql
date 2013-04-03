-- phpMyAdmin SQL Dump
-- version 3.3.9
-- http://www.phpmyadmin.net
--
-- Host: localhost
-- Czas wygenerowania: 13 Lut 2012, 20:02
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
(66666, 88888, 0),
(753, 123, 1),
(777, 555, 0),
(9876, 365, 1),
(365, 74275, 1),
(74275, 7452, 1),
(7452, 842, 1),
(842, 3241, 1),
(3241, 25745, 1),
(25745, 5734, 1),
(5734, 856, 1),
(856, 523, 1),
(523, 7892, 1),
(523, 365, 1),
(9876, 25745, 1),
(9876, 5734, 1),
(9876, 856, 1),
(9876, 523, 1),
(9876, 7892, 1),
(74253, 52367534, 1),
(999, 111, 1),
(463523, 654327, 1),
(100000907961641, 100000193797792, 1),
(100000907961641, 100001528270330, 0),
(100000907961641, 1280025315, 0),
(100000193797792, 100000907961641, 0),
(100000907961641, 1678823511, 0),
(100000907961641, 123312321, 1),
(100002016949306, 100000907961641, 0),
(100001255732296, 100000907961641, 0),
(524478444, 100000907961641, 0),
(1325800680, 100000907961641, 0),
(100002575625117, 100000907961641, 0),
(100000659197363, 100002691295261, 1),
(100000659197363, 100000907961641, 1);

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_accepted_history`
--

CREATE TABLE IF NOT EXISTS `transactions_accepted_history` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_accepted_history`
--

INSERT INTO `transactions_accepted_history` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(746254601, 100000907961641, 0, 'PLN', '2012-02-12 17:13:21', 1, ' '),
(746254601, 123312321, 0, 'PLN', '2012-02-12 17:13:21', 1, ' '),
(746254598, 100000907961641, 0, 'PLN', '2012-02-12 17:16:55', 1, ' '),
(746254598, 123312321, 0, 'PLN', '2012-02-12 17:16:55', 1, ' '),
(746254603, 100000907961641, 100, 'PLN', '2012-02-12 17:27:32', 1, ' '),
(746254603, 123312321, -100, 'PLN', '2012-02-12 17:27:32', 1, ' '),
(746254609, 100000907961641, -150, 'EUR', '2012-02-12 18:08:06', 1, ' '),
(746254609, 123312321, 150, 'EUR', '2012-02-12 18:08:06', 1, ' '),
(746254609, 0, 0, 'EUR', '2012-02-12 18:08:06', 1, 'EURO2012'),
(746254611, 100000907961641, -500, 'USD', '2012-02-13 17:13:14', 1, 'I payed 5$!'),
(746254611, 100000659197363, 1500, 'USD', '2012-02-13 17:13:14', 1, ' '),
(746254611, 100002691295261, -1000, 'USD', '2012-02-13 17:13:14', 1, ' '),
(746254611, 0, 0, 'USD', '2012-02-13 17:13:14', 1, 'musem tickets');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_cancelled`
--

CREATE TABLE IF NOT EXISTS `transactions_cancelled` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_cancelled`
--

INSERT INTO `transactions_cancelled` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(123, 234, 43, '', '0000-00-00 00:00:00', 0, ''),
(746254535, 100000907961641, 42, 'PLN', '2012-01-30 11:58:55', 1, ' '),
(746254535, 100001671486754, -42, 'PLN', '2012-01-30 11:58:55', 0, ' '),
(746254543, 100000907961641, 32, 'PLN', '2012-01-30 13:58:47', 1, ' '),
(746254543, 100001134422481, -32, 'PLN', '2012-01-30 13:58:47', 0, ' '),
(746254544, 100000907961641, 43, 'PLN', '2012-01-30 14:33:47', 1, ' '),
(746254544, 100000118177065, -43, 'PLN', '2012-01-30 14:33:47', 0, ' '),
(746254592, 100000907961641, -150, 'PLN', '2012-02-11 19:11:24', 0, ' '),
(746254592, 123312321, 150, 'PLN', '2012-02-11 19:11:24', 0, ' '),
(746254606, 100000907961641, -50, 'PLN', '2012-02-12 16:33:50', 1, ' '),
(746254606, 123312321, 50, 'PLN', '2012-02-12 16:33:50', 0, ' '),
(746254606, 0, 0, 'PLN', '2012-02-12 16:33:50', 0, ' '),
(746254607, 100000907961641, 21400, 'PLN', '2012-02-12 16:43:18', 0, ' '),
(746254607, 123312321, -21400, 'PLN', '2012-02-12 16:43:18', 0, ' '),
(746254607, 0, 0, 'PLN', '2012-02-12 16:43:18', 1, 'dupa'),
(746254593, 100000907961641, 0, 'PLN', '2012-02-12 17:11:37', 1, ' '),
(746254593, 123312321, 0, 'PLN', '2012-02-12 17:11:37', 1, ' ');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_history`
--

CREATE TABLE IF NOT EXISTS `transactions_history` (
  `transactionID` int(10) unsigned NOT NULL,
  `plusGuyID` bigint(20) unsigned NOT NULL,
  `minusGuyID` bigint(20) unsigned NOT NULL,
  `value` int(10) unsigned NOT NULL,
  `currency` tinytext NOT NULL,
  `date` datetime NOT NULL,
  `fulfillDate` datetime DEFAULT NULL,
  `desc` text,
  KEY `transactionID` (`transactionID`),
  KEY `plusGuyID` (`plusGuyID`),
  KEY `minusGuyID` (`minusGuyID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_history`
--

INSERT INTO `transactions_history` (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `fulfillDate`, `desc`) VALUES
(746254593, 100000907961641, 123312321, 2100, 'PLN', '2012-02-12 17:11:37', '2012-02-12 18:40:16', ' ');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_pending`
--

CREATE TABLE IF NOT EXISTS `transactions_pending` (
  `transactionID` int(10) unsigned NOT NULL,
  `plusGuyID` bigint(20) unsigned NOT NULL,
  `minusGuyID` bigint(20) unsigned NOT NULL,
  `value` int(10) unsigned NOT NULL,
  `currency` tinytext,
  `date` datetime DEFAULT NULL,
  `desc` text,
  KEY `plusGuyID` (`plusGuyID`),
  KEY `minusGuyID` (`minusGuyID`),
  KEY `date` (`date`),
  KEY `transactionID` (`transactionID`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_pending`
--

INSERT INTO `transactions_pending` (`transactionID`, `plusGuyID`, `minusGuyID`, `value`, `currency`, `date`, `desc`) VALUES
(7891542, 100000193797792, 100000907961641, 421, 'PLN', '2012-01-31 10:44:36', 'dupa'),
(746254601, 100000907961641, 123312321, 100, 'PLN', '2012-02-12 17:13:21', ' '),
(746254598, 123312321, 100000907961641, 16500, 'PLN', '2012-02-12 17:16:55', ' '),
(746254603, 100000907961641, 123312321, 100, 'PLN', '2012-02-12 17:27:32', ' '),
(746254609, 123312321, 100000907961641, 150, 'EUR', '2012-02-12 18:08:06', ' '),
(746254611, 100000659197363, 100002691295261, 1000, 'USD', '2012-02-13 17:13:14', ' '),
(746254611, 100000659197363, 100000907961641, 500, 'USD', '2012-02-13 17:13:14', ' ');

-- --------------------------------------------------------

--
-- Struktura tabeli dla  `transactions_reqs`
--

CREATE TABLE IF NOT EXISTS `transactions_reqs` (
  `transactionID` int(10) unsigned NOT NULL,
  `userID` bigint(20) unsigned NOT NULL,
  `value` int(11) NOT NULL,
  `currency` tinytext NOT NULL,
  `date` datetime NOT NULL,
  `accepted` smallint(6) NOT NULL,
  `desc` text NOT NULL,
  KEY `transactionID` (`transactionID`),
  KEY `userID` (`userID`),
  KEY `date` (`date`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

--
-- Zrzut danych tabeli `transactions_reqs`
--

INSERT INTO `transactions_reqs` (`transactionID`, `userID`, `value`, `currency`, `date`, `accepted`, `desc`) VALUES
(746254595, 100000907961641, 0, 'PLN', '2012-02-11 20:31:32', 1, ' '),
(746254595, 123312321, 0, 'PLN', '2012-02-11 20:31:32', 1, ' '),
(746254602, 100000907961641, 0, 'PLN', '2012-02-11 21:02:37', 1, ' '),
(746254602, 123312321, 0, 'PLN', '2012-02-11 21:02:37', 1, ' '),
(746254604, 100000907961641, 50, 'PLN', '2012-02-12 13:27:32', 1, ' '),
(746254604, 100000907961641, 50, 'PLN', '2012-02-12 13:27:32', 1, ' '),
(746254604, 123312321, -50, 'PLN', '2012-02-12 13:27:32', 1, ' '),
(746254604, 123312321, -50, 'PLN', '2012-02-12 13:27:32', 1, ' '),
(746254605, 100000907961641, 0, 'PLN', '2012-02-12 13:33:57', 1, ' '),
(746254605, 123312321, 0, 'PLN', '2012-02-12 13:33:57', 1, ' '),
(746254608, 100000907961641, -1650, 'PLN', '2012-02-12 17:50:18', 1, ' '),
(746254608, 123312321, 1650, 'PLN', '2012-02-12 17:50:18', 1, ' '),
(746254608, 0, 0, 'PLN', '2012-02-12 17:50:18', 1, 'dsads'),
(746254610, 100000907961641, 1600, 'PLN', '2012-02-12 18:13:35', 1, ' '),
(746254610, 123312321, -1600, 'PLN', '2012-02-12 18:13:35', 1, ' '),
(746254610, 0, 0, 'PLN', '2012-02-12 18:13:35', 1, '');

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
