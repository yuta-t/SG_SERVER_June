#!/usr/bin/env bash
#wait for the MySQL Server to come up
#sleep 90s

#run the setup script to create the DB and the schema in the DB
mysql -u docker -pdocker tr_test_db < "/docker-entrypoint-initdb.d/tr_test_db.sql"