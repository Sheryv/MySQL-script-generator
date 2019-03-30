# MySQL script genrator

It is a simple C# WPF app which generate MySQL create table script based on simplified data. It generates also html code structure to describe this tables 

## Download: [Latest release](https://github.com/Sheryv/MySQL-script-generator/releases/latest)


## Basic structure

### Table Name
Signs `+` or `#`
```
+<table name>, <option>\n
```
Example:
```
+Users
```
Everything below will be considered as field names until next `+` or `#` sign.
If you need to disable auto id fields add `,noid` after table name, ex.: `+Users, noid`

### Fields
```
<field name>, <type and size>, <[n]>, <[u]>, <[ref Table name.field]>, <[pk]> \n
```
 * `<field name>` -> can contain space to separate words, will be formated according to chosen mode (now there are 3 modes: `UpperCamelCase`, `loweCamelCase`, `underscore_case`)
  Example: `my new field`
  

 * `<type and size>` -> proper mysql data type, few abbreviations are available: 
    -- `v` for varchar
    Size is optional and can be provided in parentheses or after space
    Example: `int(2)`, `int 2`, `v 22`, `tinyint 3`, `datetime` 
 * `<[n]>` -> this parameter is optional. If `n` is provided it inverts option `Add NOT NULL by default`. If all fields get `NOT NULL` `n` option make marked field nullable. For foreigh keys `n` always means nullable.
 * `<[u]>` -> this parameter is optional and stand for `UNIQUE`. If `u` is present `UNIQUE` will be inserted for that field
 * `<[ref Table name.field]>` -> adds `REFERENCES Table name(field)`
 * `<[pk]>` -> makes field primary key
 
__Every field have to be defined in new line__

__Id fields defined as primary key are added automatically to every table__.  
If you need to disable this feature add `,noid` after table name, ex.: `+Users, noid`

Example fields
```
second name,v 25,n 
Email,v 255,u
```


### Html table

Application generate html table used to describe databese tables in system specification.
For now there are only Polish headers defined, to use other language constant strings in code need to be replaced.

## Full Example 
From
``` 
+ users 
Login, v 40,u 
Email,varchar(255),u 
Password,v 64 
Registered, datetime, n
Last Login, decimal(1 3), n 
Last Login In App, datetime, n 
Security Level, tinyint 2 
Permissions, int 
Phone Number, char 7, n
Verified, boolean
strange naMe Example, text, n

# RoleS, noid
custom id, int, pk
Name, v 128
Desc, longtext,n
owner user, int, n, ref users

+ users Roles, noid
UserId, int, ref Users, pk
RoleId, int, ref Roles.custom id, pk
```
To
``` sql
CREATE TABLE Users (
    Id INT AUTO_INCREMENT NOT NULL,
    Login VARCHAR(40) NOT NULL UNIQUE,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Password VARCHAR(64) NOT NULL,
    Registered DATETIME,
    LastLogin DECIMAL(1,3),
    LastLoginInApp DATETIME,
    SecurityLevel TINYINT(2) NOT NULL,
    Permissions INT NOT NULL,
    PhoneNumber CHAR(7),
    Verified BOOLEAN NOT NULL,
    StrangeNameExample TEXT,
PRIMARY KEY(Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE Roles (
    CustomId INT NOT NULL,
    Name VARCHAR(128) NOT NULL,
    Desc LONGTEXT,
    OwnerUser INT,
PRIMARY KEY(CustomId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;

CREATE TABLE UsersRoles (
    Userid INT NOT NULL,
    Roleid INT NOT NULL,
PRIMARY KEY(Userid, Roleid)
) ENGINE=InnoDB DEFAULT CHARSET=utf8;


ALTER TABLE Roles ADD CONSTRAINT RolesOwnerUser_UsersId FOREIGN KEY(OwnerUser) REFERENCES Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE UsersRoles ADD CONSTRAINT UsersRolesUserid_UsersId FOREIGN KEY(Userid) REFERENCES Users (Id) ON DELETE NO ACTION ON UPDATE NO ACTION;
ALTER TABLE UsersRoles ADD CONSTRAINT UsersRolesRoleid_RolesCustomId FOREIGN KEY(Roleid) REFERENCES Roles (CustomId) ON DELETE NO ACTION ON UPDATE NO ACTION;
```
And
```html
<html lang="pl"><head><meta charset="UTF-8"><link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/3.3.7/css/bootstrap.min.css" /></head><body>
<h2>Users</h2>
<table class="table">
<thead>
<tr>
    <th>Nazwa pola</th><th>Typ pola</th><th>Czy pole jest wymagane</th><th>Czy wartość jest unikatowa</th><th>Pozostałe atrybuty</th><th>Opis</th></tr>
</thead>
<tbody>
<tr>
    <td>Id</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>
<tr>
    <td>Login</td>
    <td>Znakowy (max. 40)</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Email</td>
    <td>Znakowy (max. 255)</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Password</td>
    <td>Znakowy (max. 64)</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Registered</td>
    <td>Data i czas</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>LastLogin</td>
    <td>Dziesiętny (max. 1,3)</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>LastLoginInApp</td>
    <td>Data i czas</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>SecurityLevel</td>
    <td>Całkowity (max. 2)</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Permissions</td>
    <td>Całkowity</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>PhoneNumber</td>
    <td>Znakowy (max. 7)</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Verified</td>
    <td>Logiczny</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>StrangeNameExample</td>
    <td>Tekstowy</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
</tbody>
</table>

<h2>Roles</h2>
<table class="table">
<thead>
<tr>
    <th>Nazwa pola</th><th>Typ pola</th><th>Czy pole jest wymagane</th><th>Czy wartość jest unikatowa</th><th>Pozostałe atrybuty</th><th>Opis</th></tr>
</thead>
<tbody>
<tr>
    <td>CustomId</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>
<tr>
    <td>Name</td>
    <td>Znakowy (max. 128)</td>
    <td>Nie</td>
    <td>Tak</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Desc</td>
    <td>LONGTEXT_#</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>OwnerUser</td>
    <td>Całkowity</td>
    <td>Nie</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
</tbody>
</table>

<h2>UsersRoles</h2>
<table class="table">
<thead>
<tr>
    <th>Nazwa pola</th><th>Typ pola</th><th>Czy pole jest wymagane</th><th>Czy wartość jest unikatowa</th><th>Pozostałe atrybuty</th><th>Opis</th></tr>
</thead>
<tbody>
<tr>
    <td>Userid</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>
<tr>
    <td>Roleid</td><td>Całkowity</td><td>Tak</td><td>Tak</td><td>Klucz podstawowy, automatyczna inkrementacja</td><td>Wewnętrzny identyfikator</td></tr>
</tbody>
</table>


</body></html>
```
