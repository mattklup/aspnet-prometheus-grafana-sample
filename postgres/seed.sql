\connect sampledb

CREATE TABLE IF NOT EXISTS users
(
    id serial PRIMARY KEY,
    name  VARCHAR (50)  NOT NULL,
    description  VARCHAR (100)  NOT NULL
);

ALTER TABLE "users" OWNER TO postgres;

Insert into users(name,description) values('User 1', 'Description 1');
Insert into users(name,description) values('User 2', 'Description 2');
Insert into users(name,description) values('User 3', 'Description 3');
