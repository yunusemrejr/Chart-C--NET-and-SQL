CREATE SCHEMA `demo1` ;

use demo1;
CREATE TABLE `demo1`.`table1` (
ID int not null primary key,
isim varchar(50),
price float

);


INSERT INTO `demo1`.`table1` (`ID`, `isim`, `price`) VALUES ('1', 'item1', '23.43');
INSERT INTO `demo1`.`table1` (`ID`, `isim`, `price`) VALUES ('2', 'item2', '5.43');
INSERT INTO `demo1`.`table1` (`ID`, `isim`, `price`) VALUES ('3', 'item3', '12.55');
INSERT INTO `demo1`.`table1` (`ID`, `isim`, `price`) VALUES ('4', 'item4', '21.32');
INSERT INTO `demo1`.`table1` (`ID`, `isim`, `price`) VALUES ('5', 'item5', '13.77');
