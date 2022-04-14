DROP TABLE onerow;
CREATE TABLE onerow (
   onerow_id bool PRIMARY KEY DEFAULT TRUE
 , data text
 , CONSTRAINT onerow_uni CHECK (onerow_id)
);

insert into onerow (data) values ( 'one');
comment on table onerow is 'constraint on primary key limits to one row';
select * from onerow; 