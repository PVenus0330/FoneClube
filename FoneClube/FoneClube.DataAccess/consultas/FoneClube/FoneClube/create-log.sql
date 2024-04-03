-----------------------------------
----log----------------------------
-----------------------------------

--select * from tblLog

create table tblLogTipo
(
    intIdTipo int IDENTITY(1,1) primary key not null,
	txtDescricao nvarchar(250) not null
);

create table tblLog
(
    intId int  IDENTITY(1,1) primary key not null,
    intIdTipo int FOREIGN KEY REFERENCES tblLogTipo(intIdTipo),
	txtAction nvarchar(250) not null,
	dteTimeStamp datetime not null
);



insert into tblLogTipo (txtDescricao)
values ('Dev Debug')

insert into tblLogTipo (txtDescricao)
values ('Relatório de Erro')

insert into tblLogTipo (txtDescricao)
values ('Parseamento')
