IF EXISTS(SELECT * FROM tbl_monitor WHERE line='FA-01-3555' AND mc='Body-Bone Welding')
   UPDATE tbl_monitor SET actual='4', ok='2', ng='1' WHERE line='FA-01-3555' AND mc='Body-Bone Welding' 
ELSE
   INSERT INTO tbl_monitor(actual, ok, ng,line,mc,reset) VALUES('5','2','0','FA-01-3555','Body-Bone Welding','2023-08-03 14:00:00.000');