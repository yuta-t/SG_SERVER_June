#!/bin/bash
set -e

echo "Running initialization script..."

mysql -u root -p"${MYSQL_ROOT_PASSWORD}" tr_test_db < "/docker-entrypoint-initdb.d/tr_test_db.sql"

# MySQL に接続して初期化SQLを実行する場合
# mysql -u root -p"${MYSQL_ROOT_PASSWORD}" <<EOSQL
# GRANT SYSTEM_USER TO 'docker'@'localhost';
# FLUSH PRIVILEGES;
# EOSQL

echo "Initialization completed."