SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Records of demo_class
-- ----------------------------
INSERT INTO `demo_class` VALUES ('A001', '班级001', '1', '1');
INSERT INTO `demo_class` VALUES ('A002', '班级002', '1', '2');
INSERT INTO `demo_class` VALUES ('A003', '班级003', '2', '1');

-- ----------------------------
-- Records of demo_course
-- ----------------------------
INSERT INTO `demo_course` VALUES ('2001', '7CF78E4B-D2EC-4B18-926E-CF3503D64FDD', '语文', '1');
INSERT INTO `demo_course` VALUES ('2001', '86110DD4-4254-49A2-A312-1DAC92A87EC1', '数学', '2');
INSERT INTO `demo_course` VALUES ('2002', 'EC312A0B-8243-4ABE-95DD-B0F9B4B8BB0D', '英语', '3');

-- ----------------------------
-- Records of demo_user
-- ----------------------------
INSERT INTO `demo_user` VALUES ('1', 'A001', '\0', null, '127', '255', '1', '0', null, '-32768', '65535', '-8388608', '16777215', '-2147483648', '4294967295', null, '-9223372036854775808', '18446744073709551615', '12.3457', '12.3457', '123456789.1234567', null, null, '12.34', '2001', '2001-02-03', '12:34:56', '2017-11-30 10:07:57', '2001-02-03 04:05:06', 'A01', 'B02', null, null, 'aaa', 'bbb', 'ccc', 'ddd', null, null, null, null, 'f', 'two', '12345', '123.456000000000000000000000000000', '12345', '12345', '12345');
INSERT INTO `demo_user` VALUES ('2', 'A001', '', '\0\0\0\0\0\0\0\0', '-128', null, '0', '0', '0', '32767', null, '8388607', null, '2147483647', null, '-1', '9223372036854775807', null, '123.457', '123.4568', '123.123456789', '12.3400', '56.78', null, null, null, null, '2017-11-21 19:59:08', null, null, null, null, null, null, null, null, null, null, null, null, null, 'f', 'one', '321', null, null, null, null);
INSERT INTO `demo_user` VALUES ('3', 'A002', '', null, '-1', '1', null, '0', null, '0', null, '-1', null, '-1', '0', null, '-1', '0', '2.2', null, '123456789.1234567', null, null, null, null, null, null, '2017-11-21 19:59:20', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, '1', null, null, null, null);
INSERT INTO `demo_user` VALUES ('4', 'A003', '', null, '-2', '2', null, '1', null, '-1', null, '-2', null, '-2', null, null, '-2', null, '3.3', null, '123456789.1234567', null, null, null, null, null, null, '2017-11-21 19:59:20', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, '2', null, null, null, null);
INSERT INTO `demo_user` VALUES ('5', 'A003', '', null, '-3', '3', null, '0', null, '-2', null, '-3', null, '-3', null, null, '-3', null, '4.4', null, '123456789.1234567', null, null, null, null, null, null, '2017-11-21 19:59:21', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, '3', null, null, null, null);
INSERT INTO `demo_user` VALUES ('6', 'A003', '', null, '-4', '4', null, '1', null, '-3', null, '-4', null, '-4', null, null, '-4', null, '5.5', null, '123456789.1234567', null, null, null, null, null, null, '2017-11-21 19:59:21', null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, '4', null, null, null, null);


-- ----------------------------
-- Records of demo_user_course
-- ----------------------------
INSERT INTO `demo_user_course` VALUES ('1', '2001', '7CF78E4B-D2EC-4B18-926E-CF3503D64FDD', 'aaa');
INSERT INTO `demo_user_course` VALUES ('1', '2001', '86110DD4-4254-49A2-A312-1DAC92A87EC1', 'ccc');
INSERT INTO `demo_user_course` VALUES ('2', '2001', '7CF78E4B-D2EC-4B18-926E-CF3503D64FDD', 'bbb');
INSERT INTO `demo_user_course` VALUES ('2', '2001', '86110DD4-4254-49A2-A312-1DAC92A87EC1', 'eee');
INSERT INTO `demo_user_course` VALUES ('2', '2002', 'EC312A0B-8243-4ABE-95DD-B0F9B4B8BB0D', 'ttt');
INSERT INTO `demo_user_course` VALUES ('3', '2002', 'EC312A0B-8243-4ABE-95DD-B0F9B4B8BB0D', 'ggg');

DELIMITER ;
