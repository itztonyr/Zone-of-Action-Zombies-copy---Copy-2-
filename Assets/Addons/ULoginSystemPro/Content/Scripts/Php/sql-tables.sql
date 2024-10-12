CREATE TABLE `bl_game_users` (
  `id` int(10) NOT NULL,
  `name` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  `nick` varchar(20) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  `password` varchar(64) NOT NULL,
  `kills` int(11) UNSIGNED NOT NULL DEFAULT 0,
  `deaths` int(11) UNSIGNED NOT NULL DEFAULT 0,
  `score` int(11) UNSIGNED NOT NULL DEFAULT 0,
  `assist` int(11) UNSIGNED NOT NULL DEFAULT 0,
  `coins` varchar(20) DEFAULT NULL,
  `purchases` text DEFAULT NULL,
  `meta` text CHARACTER SET utf8 COLLATE utf8_unicode_ci,
  `clan` varchar(12) NOT NULL DEFAULT '-1',
  `clan_invitations` varchar(50) NOT NULL DEFAULT '-1,',
  `playtime` int(64) UNSIGNED NOT NULL DEFAULT '0',
  `email` varchar(128) DEFAULT NULL,
  `active` int(1) NOT NULL DEFAULT '0',
  `ip` varchar(128) NOT NULL DEFAULT 'none',
  `friends` varchar(252) DEFAULT NULL,
  `status` int(3) NOT NULL DEFAULT '0',
  `verify` varchar(32) DEFAULT NULL,
  `user_date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

ALTER TABLE `bl_game_users`
  ADD PRIMARY KEY (`id`),
  ADD KEY `name` (`name`),
  ADD KEY `nick` (`nick`),
  ADD KEY `id` (`id`);

ALTER TABLE `bl_game_users`
  MODIFY `id` int(10) NOT NULL AUTO_INCREMENT, AUTO_INCREMENT=0;
COMMIT;

CREATE TABLE `bl_game_tickets` (
  `id` int(11) NOT NULL,
  `user_id` int(11) UNSIGNED NOT NULL,
  `title` varchar(256) NOT NULL,
  `chat` text DEFAULT NULL,
  `status` tinyint(3) NOT NULL DEFAULT 0,
  `last_update` timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp(),
  `created_date` timestamp NOT NULL DEFAULT current_timestamp()
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

ALTER TABLE `bl_game_tickets`
  ADD PRIMARY KEY (`id`),
  ADD UNIQUE KEY `id` (`id`),
  ADD KEY `status` (`status`);

CREATE TABLE `bl_game_bans` (
  `id` int(10) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `user_id` int(10) NOT NULL,
  `name` varchar(30) CHARACTER SET latin1 COLLATE latin1_general_cs NOT NULL,
  `reason` varchar(128) NOT NULL,
  `by` varchar(32) NOT NULL,
  `ip` varchar(128) DEFAULT NULL,
  `device_id` varchar(256) DEFAULT NULL,
  `date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

CREATE TABLE `bl_game_purchases` (
  `id` int(11) NOT NULL AUTO_INCREMENT PRIMARY KEY,
  `product_id` varchar(70) NOT NULL,
  `receipt` text NOT NULL,
  `user_id` int(11) NOT NULL,
  `date` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=latin1;