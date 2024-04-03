-------------------------------------------
-- persons --------------------------------
-------------------------------------------

create table tblPersons
(
    intIdPerson int IDENTITY(1,1) primary key,
	intContactId int,
    dteRegister datetime not null,
    txtDocumentNumber nvarchar(15) not null,
	txtName nvarchar(60) not null,
	txtNickName nvarchar(60),
	txtEmail nvarchar(30),
	dteBorn date,
	intGender int,
	intIdPagarme int,
	intIdRole int,
	intIdCurrentOperator int,
	matricula bigint
);



create table tblPersonsPhones
(
    intId int IDENTITY(1,1) primary key,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
	intDDD int,
	intPhone bigint,
	intIdOperator int,
	bitPhoneClube bit
);

create table tblPersonsAddresses
(
    intId int IDENTITY(1,1) primary key,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
	txtStreet nvarchar(60),
	txtComplement nvarchar(60),
	intStreetNumber int,
	txtNeighborhood nvarchar(60),
	txtCity nvarchar(60),
	txtState nvarchar(20),
	txtCep nvarchar(20)  not null,
	txtCountry nvarchar(20),
);

create table tblPersonsImages
(
    intId int IDENTITY(1,1) primary key,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson),
	txtImage nvarchar(80) not null
);


----------------------------------------
---- operadoras, taxas, planos
----------------------------------------



create table tblPlansOptions(
	intIdPlan int primary key not null,
	intIdOperator int FOREIGN KEY REFERENCES tblOperadoras(intIdOperator) not null,
	txtDescription nvarchar(60) not null,
	intCost int not null,
	intBitActive bit
)

create table tblPlans(
	intIdPlan int IDENTITY(1,1) primary key,
	intIdOption int FOREIGN KEY REFERENCES tblPlansOptions(intIdPlan),
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson)
)

create table tblCommissions
(
    intIdComission int primary key not null,
    intComissionCost int not null
)

create table tblReferred
(
    intId int IDENTITY(1,1) primary key not null,
    intIdComission int FOREIGN KEY REFERENCES tblCommissions(intIdComission) not null,
    intIdDad int FOREIGN KEY REFERENCES tblPersons(intIdPerson) not null,
	intIdCurrent int FOREIGN KEY REFERENCES tblPersons(intIdPerson) not null,
)

create table tblCommissionOrders
(
    intIdComissionOrder int IDENTITY(1,1) primary key not null,
    intIdComission int FOREIGN KEY REFERENCES tblCommissions(intIdComission) not null,
	intIdPerson int FOREIGN KEY REFERENCES tblPersons(intIdPerson) not null,
	intBitActive bit not null,
	dteCreated datetime not null
)

create table tblOperadoras
(
    intIdOperator int primary key not null,
    txtName nvarchar(10) not null
);

drop table tblCommissionOrders
drop table tblReferred
drop table tblCommissions

drop table tblPlans 
drop table tblPlansOptions

drop table tblPersonsAddresses
drop table tblPersonsPhones
drop table tblPersonsImages
drop table tblPersons

drop table tblOperadoras

