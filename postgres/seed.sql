
\connect sampledb

CREATE TABLE sample
(
    id serial PRIMARY KEY,
    title  VARCHAR (50)  NOT NULL,
    description  VARCHAR (100)  NOT NULL
);

ALTER TABLE "sample" OWNER TO dbuser;

Insert into sample(title,description) values( 'Title 1','Description 1');
Insert into sample(title,description) values( 'Title 2','Description 2');
Insert into sample(title,description) values( 'Title 3','Description 3');
