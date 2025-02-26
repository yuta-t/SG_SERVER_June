# Strugarden Server Database
ストラガーデンサーバでは、ユーザデータ等を参照するためにデータベースを利用します。
このときMySQLを利用しますが、環境の構築を容易にするために動作するdocker-composeを用意しました。
 
# Requirement
以下の環境で動作確認しました。 

* Docker version 27.5.1, build 9f9e405
* Docker Compose version v2.32.4-desktop.1
 
バージョン依存性が強いものは利用していないため、docker-composeが動けば大体動くと思います。

# Structure
```
├── docker
│   └── db
│       ├── data                        <-ここにsqlのデータが作成される(/var/lib/mysql)
│       ├── my.cnf                      <-(/etc/mysql/conf.d/my.cnf)
│       └── sql                         <-(/docker-entrypoint-initdb.d)
│           ├── tr_test_db.sql          <-初期データのテンプレート
│           └── init-database.sh        <-初期化コマンド
├── docker-compose.yml
└── readme.md
```

# Usage

## 初回起動時のセットアップ 
```bash
database$ docker-compose up -d
database$ docker-compose ps         #起動確認
```
ここで
```
NAME              IMAGE                   ...
mysql_host        mysql:5.7               ...
test_phpmyadmin   phpmyadmin/phpmyadmin   ...
```
と2つが表示されなければ、``docker-compose up -d``を再度実行してください。  
(特に初回はmysql_hostが立ち上がらない事があります。)
2つが起動されたら
```bash
database$ docker-compose exec db bash -c "chmod 0775 docker-entrypoint-initdb.d/init-database.sh"
database$ docker-compose exec db bash -c "./docker-entrypoint-initdb.d/init-database.sh"
```
を実行することでtr_test_db.sqlをもとに初期化します。  
またphpMyAdminを用いて、
http://localhost:8080/index.php
から確認することができます。

## コマンド類
- サーバ起動 
```bash
database$ docker-compose up -d
```
- 確認 
```bash
database$ docker-compose ps
```

- サーバ停止
```bash
database$ docker-compose stop
```

- データベース初期化
```bash
database$ docker-compose exec db bash -c "chmod 0775 docker-entrypoint-initdb.d/init-database.sh"
database$ docker-compose exec db bash -c "./docker-entrypoint-initdb.d/init-database.sh"
```

- dbサーバを直接操作(rootログイン)
```bash
database$ docker-compse exec db bash
```



# Note
次のサイトをMySQL構築の際に参考にしました
- docker-compose でMySQL環境簡単構築
    - https://qiita.com/A-Kira/items/f401aea261693c395966

rootのPWについては「PWなし」の設定が面倒だったため、「root!」としております。  
``server=127.0.0.1;port=3306;user id=root;password=root!;database=tr_test_db;charset=utf8mb4;``  
を用いて接続してください。