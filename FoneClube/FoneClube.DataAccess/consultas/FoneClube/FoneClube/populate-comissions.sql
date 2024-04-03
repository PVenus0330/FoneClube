--- commissions static

insert into tblCommissions(intIdComission, intComissionCost)
values(1,500)

insert into tblCommissions(intIdComission, intComissionCost)
values(2,300)

insert into tblCommissions(intIdComission, intComissionCost)
values(3,200)


--- referred to test

insert into tblReferred(intIdComission, intIdDad, intIdCurrent)
values (1,3,4)

insert into tblReferred(intIdComission, intIdDad, intIdCurrent)
values (2,2,4)

insert into tblReferred(intIdComission, intIdDad, intIdCurrent)
values (3,1,4)

------------------

select* from tblReferred
truncate table tblReferred


select * from tblCommissions
truncate table tblCommissions