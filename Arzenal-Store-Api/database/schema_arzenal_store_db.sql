-- MySQL dump 10.13  Distrib 9.0.1, for Linux (x86_64)
--
-- Host: localhost    Database: arzenal_store_db
-- ------------------------------------------------------
-- Server version	9.0.1

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `AppLanguages`
--

DROP TABLE IF EXISTS `AppLanguages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AppLanguages` (
  `AppId` char(36) NOT NULL,
  `LanguageId` int NOT NULL,
  PRIMARY KEY (`AppId`,`LanguageId`),
  KEY `AppLanguages_ibfk_2` (`LanguageId`),
  CONSTRAINT `AppLanguages_ibfk_1` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `AppLanguages_ibfk_2` FOREIGN KEY (`LanguageId`) REFERENCES `Languages` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AppOperatingSystems`
--

DROP TABLE IF EXISTS `AppOperatingSystems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AppOperatingSystems` (
  `AppId` char(36) NOT NULL,
  `OSId` int NOT NULL,
  PRIMARY KEY (`AppId`,`OSId`),
  KEY `FK_AppOperatingSystems_OperatingSystems_OSId` (`OSId`),
  CONSTRAINT `AppOperatingSystems_ibfk_1` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `AppOperatingSystems_ibfk_2` FOREIGN KEY (`OSId`) REFERENCES `OperatingSystems` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AppOperatingSystems_Apps_AppId` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AppOperatingSystems_OperatingSystems_OSId` FOREIGN KEY (`OSId`) REFERENCES `OperatingSystems` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `AppTags`
--

DROP TABLE IF EXISTS `AppTags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `AppTags` (
  `AppId` char(36) NOT NULL,
  `TagId` int NOT NULL,
  PRIMARY KEY (`AppId`,`TagId`),
  KEY `AppTags_ibfk_2` (`TagId`),
  CONSTRAINT `AppTags_ibfk_1` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`),
  CONSTRAINT `AppTags_ibfk_2` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AppTags_Apps_AppId` FOREIGN KEY (`AppId`) REFERENCES `Apps` (`Id`) ON DELETE CASCADE,
  CONSTRAINT `FK_AppTags_Tags_TagId` FOREIGN KEY (`TagId`) REFERENCES `Tags` (`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Apps`
--

DROP TABLE IF EXISTS `Apps`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Apps` (
  `Id` char(36) NOT NULL,
  `Name` varchar(255) NOT NULL,
  `Version` varchar(50) NOT NULL,
  `FilePath` varchar(255) NOT NULL,
  `Description` text,
  `IsVisible` tinyint(1) DEFAULT '1',
  `Icone` varchar(255) DEFAULT NULL,
  `ReleaseDate` datetime DEFAULT CURRENT_TIMESTAMP,
  `LastUpdated` datetime DEFAULT NULL,
  `AppSize` bigint DEFAULT NULL,
  `Requirements` text,
  `CategoryId` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `FK_Category` (`CategoryId`),
  CONSTRAINT `FK_Category` FOREIGN KEY (`CategoryId`) REFERENCES `Categories` (`Id`) ON DELETE SET NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Categories`
--

DROP TABLE IF EXISTS `Categories`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Categories` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=301 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Languages`
--

DROP TABLE IF EXISTS `Languages`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Languages` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=587 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `OperatingSystems`
--

DROP TABLE IF EXISTS `OperatingSystems`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `OperatingSystems` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(255) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=605 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Table structure for table `Tags`
--

DROP TABLE IF EXISTS `Tags`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `Tags` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Name` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=581 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-05-28 17:28:06
