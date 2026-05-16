-- phpMyAdmin SQL Dump
-- version 5.2.1
-- https://www.phpmyadmin.net/
--
-- Host: 127.0.0.1
-- Generation Time: May 16, 2026 at 08:01 AM
-- Server version: 10.4.32-MariaDB
-- PHP Version: 8.0.30

SET SQL_MODE = "NO_AUTO_VALUE_ON_ZERO";
START TRANSACTION;
SET time_zone = "+00:00";


/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!40101 SET NAMES utf8mb4 */;

--
-- Database: `news_portal`
--

-- --------------------------------------------------------

--
-- Table structure for table `articles`
--

CREATE TABLE `articles` (
  `id` int(11) NOT NULL,
  `title` varchar(255) NOT NULL,
  `content` text NOT NULL,
  `author_id` int(11) DEFAULT NULL,
  `category_id` int(11) DEFAULT NULL,
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `articles`
--

INSERT INTO `articles` (`id`, `title`, `content`, `author_id`, `category_id`, `created_at`) VALUES
(12, 'Government Announces New Policy', 'The government has introduced a new policy aimed at economic growth.', 1, 1, '2026-04-17 04:17:54'),
(13, 'Local Team Wins Championship', 'The local football team secured a historic victory in the finals.', 1, 2, '2026-04-17 04:17:54'),
(14, 'New Smartphone Released', 'A leading tech company has launched its latest smartphone model.', 1, 3, '2026-04-17 04:17:54'),
(15, 'Celebrity Stars in New Movie', 'A famous actor is starring in an upcoming blockbuster film.', 1, 4, '2026-04-17 04:17:54'),
(16, 'Election Results Declared', 'The election commission has officially announced the results.', 1, 1, '2026-04-17 04:17:54'),
(17, 'International Match Highlights', 'An exciting cricket match kept fans on the edge of their seats.', 1, 2, '2026-04-17 04:17:54'),
(18, 'AI Technology Advancements', 'New developments in AI are transforming industries worldwide.', 1, 3, '2026-04-17 04:17:54'),
(19, 'Music Festival Announced', 'A major music festival will take place next month.', 1, 4, '2026-04-17 04:17:54'),
(20, 'Parliament Session Updates', 'Key discussions were held during today’s parliament session.', 1, 1, '2026-04-17 04:17:54'),
(21, 'Olympics Preparation Begins', 'Athletes are preparing for the upcoming Olympic games.', 1, 2, '2026-04-17 04:17:54'),
(22, 'Cybersecurity Concerns Rise', 'Experts warn about increasing cybersecurity threats globally.', 1, 3, '2026-04-17 04:17:54'),
(23, 'AI Technology Advancements', 'New developments in AI are changing industries worldwide.', 1, 3, '2026-05-05 12:08:45'),
(24, 'Local Team Wins Championship', 'The local football team secured a historic victory in the finals.', 1, 2, '2026-05-05 12:10:43'),
(25, 'International Cricket Match Highlights', 'An exciting cricket match kept fans on the edge of their seats.', 1, 2, '2026-05-05 12:10:43'),
(26, 'Basketball League Final Results', 'The final match delivered an intense competition between both teams.', 1, 2, '2026-05-05 12:10:43'),
(27, 'Local Team Wins Championship', 'The local football team secured a historic victory in the finals.', 1, 2, '2026-05-05 12:16:19'),
(28, 'International Cricket Match Highlights', 'An exciting cricket match kept fans on the edge of their seats.', 1, 2, '2026-05-05 12:16:19'),
(29, 'Basketball League Final Results', 'The final match delivered an intense competition between both teams.', 1, 2, '2026-05-05 12:16:19'),
(30, 'Government Announces New Policy', 'The government has introduced a new national policy focused on economic development and infrastructure growth.', 1, 4, '2026-05-12 16:03:18'),
(31, 'Parliament Session Updates', 'Important discussions were held in parliament regarding education reform and public sector investment.', 1, 4, '2026-05-12 16:03:18'),
(32, 'Election Results Declared', 'The election commission officially announced the final election results across all constituencies.', 1, 4, '2026-05-12 16:03:18'),
(33, 'Opposition Raises Concerns', 'Opposition leaders raised concerns regarding budget allocation and policy transparency.', 1, 4, '2026-05-12 16:03:18'),
(34, 'Foreign Relations Meeting Held', 'Government officials held international discussions to strengthen diplomatic and trade relations.', 1, 4, '2026-05-12 16:03:18'),
(35, 'NASA Discovers New Planet', 'Astronomers have discovered a new Earth-like planet that may support life.', 1, 5, '2026-05-12 16:06:00'),
(36, 'Breakthrough in Cancer Research', 'Scientists have developed a promising treatment method in early-stage clinical trials.', 1, 5, '2026-05-12 16:06:00'),
(37, 'Ocean Exploration Mission', 'Researchers have identified several unknown marine species during deep ocean exploration.', 1, 5, '2026-05-12 16:06:00'),
(38, 'Advancements in Renewable Energy', 'Scientists are improving solar panel efficiency using new materials and technologies.', 1, 5, '2026-05-12 16:06:00'),
(39, 'AI Supports Scientific Research', 'Artificial intelligence is helping researchers analyze complex scientific data faster.', 1, 5, '2026-05-12 16:06:00'),
(40, 'International Film Festival Begins', 'Film directors and actors from around the world gathered for the annual international film festival.', 1, 7, '2026-05-16 05:38:21'),
(41, 'Fashion Week Showcases New Trends', 'Designers introduced their latest fashion collections during this year’s fashion week.', 1, 7, '2026-05-16 05:38:21'),
(42, 'Local Art Exhibition Opens', 'Artists displayed modern and traditional artwork at the city cultural center.', 1, 7, '2026-05-16 05:38:21'),
(43, 'Music Concert Draws Huge Crowd', 'Thousands of music fans attended the live concert featuring popular performers.', 1, 7, '2026-05-16 05:38:21'),
(44, 'Cultural Heritage Program Launched', 'The government launched a new program to preserve local cultural heritage and traditions.', 1, 7, '2026-05-16 05:38:21'),
(45, 'Editorial: The Future of Digital Education', 'Experts discuss how digital platforms are reshaping the future of learning and education systems.', 1, 8, '2026-05-16 05:39:37'),
(46, 'Opinion: Balancing Technology and Privacy', 'Writers debate how governments and companies should balance innovation with personal privacy rights.', 1, 8, '2026-05-16 05:39:37'),
(47, 'Policy Analysis: Economic Reform Strategies', 'Analysts examine the long-term impact of proposed economic reforms on businesses and citizens.', 1, 8, '2026-05-16 05:39:37'),
(48, 'Social Debate: Remote Work Culture', 'Professionals share different perspectives on the rise of remote work and workplace flexibility.', 1, 8, '2026-05-16 05:39:37'),
(49, 'Opinion: Preparing for an AI-Driven Future', 'Industry leaders discuss how societies can adapt to rapid changes brought by artificial intelligence.', 1, 8, '2026-05-16 05:39:37');

-- --------------------------------------------------------

--
-- Table structure for table `categories`
--

CREATE TABLE `categories` (
  `id` int(11) NOT NULL,
  `name` varchar(100) NOT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `categories`
--

INSERT INTO `categories` (`id`, `name`) VALUES
(1, 'Politics'),
(2, 'Sports'),
(3, 'Technology'),
(4, 'Entertainment'),
(5, 'Tech'),
(6, 'Sports'),
(7, 'Politics'),
(8, 'Science'),
(9, 'Culture'),
(10, 'Opinion');

-- --------------------------------------------------------

--
-- Table structure for table `users`
--

CREATE TABLE `users` (
  `id` int(11) NOT NULL,
  `first_name` varchar(100) NOT NULL,
  `last_name` varchar(100) NOT NULL,
  `email` varchar(255) NOT NULL,
  `phone` varchar(20) DEFAULT NULL,
  `address` text DEFAULT NULL,
  `country` varchar(100) DEFAULT NULL,
  `password` varchar(255) NOT NULL,
  `role` varchar(20) DEFAULT 'user',
  `created_at` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;

--
-- Dumping data for table `users`
--

INSERT INTO `users` (`id`, `first_name`, `last_name`, `email`, `phone`, `address`, `country`, `password`, `role`, `created_at`) VALUES
(1, 'Admin', 'User', 'admin@gmail.com', NULL, NULL, NULL, '123456', 'user', '2026-04-17 04:16:30');

--
-- Indexes for dumped tables
--

--
-- Indexes for table `articles`
--
ALTER TABLE `articles`
  ADD PRIMARY KEY (`id`),
  ADD KEY `fk_author` (`author_id`),
  ADD KEY `fk_category` (`category_id`);

--
-- Indexes for table `categories`
--
ALTER TABLE `categories`
  ADD PRIMARY KEY (`id`);

--
-- Indexes for table `users`
--
ALTER TABLE `users`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `email` (`email`);

--
-- AUTO_INCREMENT for dumped tables
--

--
-- AUTO_INCREMENT for table `articles`
--
ALTER TABLE `articles`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=50;

--
-- AUTO_INCREMENT for table `categories`
--
ALTER TABLE `categories`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=11;

--
-- AUTO_INCREMENT for table `users`
--
ALTER TABLE `users`
  MODIFY `id` int(11) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=2;

--
-- Constraints for dumped tables
--

--
-- Constraints for table `articles`
--
ALTER TABLE `articles`
  ADD CONSTRAINT `fk_author` FOREIGN KEY (`author_id`) REFERENCES `users` (`id`),
  ADD CONSTRAINT `fk_category` FOREIGN KEY (`category_id`) REFERENCES `categories` (`id`);
COMMIT;

/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
