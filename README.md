# MySQL script genrator

It is a simple C# WPF app which generate MySQL create table script based on simplified data. It generates also html code structure to describe this tables 

## Download: [Latest release](https://github.com/Sheryv/MySQL-script-generator/releases/latest)


## Basic structure

### Table Name
```
+<table name>\n
```
Example:
```
+Users
```
Everything below will be considered as table names until next `+` sign.

### Fields
```
<field name>, <type and size>, <[n]>,<[u]> \n
```
 * `<field name>` -> can contain space to separate words, will be formated according to chosen mode (now there are 2 modes: `UpperCamelCase`, `loweCamelCase`)
  Example: `my new field`
  

 * `<type and size>` -> proper mysql data type, few abbreviations are available: 
    -- `v` for varchar
    Size is optional and can be provided in parentheses or after space
    Example: `int(2)`, `int 2`, `v 22`, `tinyint 3`, `datetime` 
 * `<[n]>` -> this parameter is optional and stand for `NULL`. If `n` is missing `NOT NULL` will be inserted for that field
 * `<[u]>` -> this parameter is optional and stand for `UNIQUE`. If `u` is present `UNIQUE` will be inserted for that field

__Every field have to be defined in new line__

__Id fields defined as primary key are added automatically to every table__

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
+Users 
Login, v 40,u 
Email,v 255,n,u 
password,v 64 
registered, datetime 
Last Login, datetime 
last login in app,datetime 
Last Synchronization, datetime 
securityLevel,tinyint 2 
Permissions, int 
Phone Number,v 20 
```
To
``` sql
CREATE TABLE Users (
    Id INT NOT NULL PRIMARY KEY AUTO_INCREMENT,
    Login VARCHAR(40) NOT NULL UNIQUE,
    Email VARCHAR(255) UNIQUE,
    Password VARCHAR(64) NOT NULL,
    Registered DATETIME NOT NULL,
    LastLogin DATETIME NOT NULL,
    LastLoginInApp DATETIME NOT NULL,
    LastSynchronization DATETIME NOT NULL,
    SecurityLevel TINYINT(2) NOT NULL,
    Permissions INT NOT NULL,
    PhoneNumber VARCHAR(20) NOT NULL
);
```
And
```html
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
    <td>Tak</td>
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
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Registered</td>
    <td>datetime_#</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>LastLogin</td>
    <td>datetime_#</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>LastLoginInApp</td>
    <td>datetime_#</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>LastSynchronization</td>
    <td>datetime_#</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>SecurityLevel</td>
    <td>Całkowity (max. 2)</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>Permissions</td>
    <td>Całkowity</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
<tr>
    <td>PhoneNumber</td>
    <td>Znakowy (max. 20)</td>
    <td>Tak</td>
    <td>Nie</td>
    <td></td>
    <td></td>
</tr>
</tbody>
</table>
```
