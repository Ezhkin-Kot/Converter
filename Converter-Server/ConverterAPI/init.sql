create table if not exists sessions
(
    sessionid serial primary key,
    userid    integer,
    datetime  timestamp with time zone,
    amount    integer,
    active    boolean
);

create table if not exists users
(
    id       serial primary key,
    login    varchar(80),
    password varchar(80),
    premium  boolean,
    salt     varchar(80)
);