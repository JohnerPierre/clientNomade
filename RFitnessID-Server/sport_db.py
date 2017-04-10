import sqlite3
import json
import os.path
from datetime import datetime

def create_sport_db(filename):
    if os.path.isfile(filename):
        raise Exception("File already exists")
    with sqlite3.connect(filename) as con:
        cur = con.cursor()
        try:
            req = "CREATE TABLE 'users' ('id' TEXT PRIMARY KEY NOT NULL, 'name' TEXT NOT NULL DEFAULT (null), 'age' INTEGER NOT NULL DEFAULT (0))"
            cur.execute(req)
            req = "CREATE TABLE 'equip_type' ('id' INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 'name' TEXT NOT NULL)"
            cur.execute(req)
            req = "CREATE TABLE 'equipement' ('id' INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL, 'name' TEXT NOT NULL, 'type' INTEGER NOT NULL, 'instructions' TEXT)"
            cur.execute(req)
            req = "CREATE TABLE 'history' ('user_id' TEXT NOT NULL , 'equip_id' INTEGER NOT NULL , 'date' TEXT NOT NULL , 'data' TEXT)"
            cur.execute(req)
        except sqlite3.Error as err:
            return "{}".format(err)
    return 0

class Sport_db(object):
    def __init__(self, file=None):
        if file != None:
            self.file = file

    '''
    SQL GENERAL
    '''
    def sql_request(self,req):
        #con = sqlite3.connect(self.file)
        rows = None
        with sqlite3.connect(self.file) as con:
            cur = con.cursor()
            try:
                cur.execute(req)
                rows = cur.fetchall()
            except sqlite3.Error as err:
                return "{}".format(err)
        return rows

    def list_table(self,table):
        req = "SELECT * FROM {}".format(table)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        return json.dumps(ret)

    '''
    GET REQUESTS
    '''
    def get_user(self,id):
        req = "SELECT * FROM users WHERE id='{}'".format(id)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        if not ret:
            return {"error":"Wrong id", "value":id}
        return {"id":ret[0][0], "name":ret[0][1], "age":ret[0][2]}

    def get_equip(self, id):
        req = "SELECT * FROM equipement WHERE id={}".format(id)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        if not ret:
            return {"error":"Wrong id", "value":id}
        typ = self.get_equip_type(ret[0][2])
        return {"id":ret[0][0], "name":ret[0][1], "type":typ["name"], "type_id":typ["id"], "instructions":ret[0][3]}

    def get_equip_type(self, id):
        req = "SELECT * FROM equip_type WHERE id={}".format(id)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        if not ret:
            return {"error":"Wrong id", "value":id}
        return {"id":ret[0][0], "name":ret[0][1]}

    def get_type_nb(self):
        req = "SELECT COUNT(id) FROM equip_type"
        ret = self.sql_request(req)
        return {"retval":ret[0][0]}

    def get_type_name(self, type_id):
        req = "SELECT name FROM equip_type WHERE id={}".format(type_id)
        ret = self.sql_request(req)
        return {"retval":ret[0][0]}

    def get_history(self, user_id):
        req = "SELECT * FROM history WHERE user_id='{}'".format(user_id)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        if not ret:
            return {"error":"History doesn't exists", "user_id":user_id}
        dic = {"user":self.get_user(user_id)}
        machine = self.get_equip(ret[0][1])
        hist = []
        for row in ret:
            hist.append({"equip":machine, "date":row[2], "data":row[3]})
        dic["history"] = hist
        return dic

    def get_history2(self, user_id, equip_id):
        req = "SELECT * FROM history WHERE user_id='{}'AND equip_id={}".format(user_id, equip_id)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        if not ret:
            return {"error":"History doesn't exists", "user_id":user_id, "machine":self.get_equip(equip_id)}
        machine = self.get_equip(ret[0][1])

        dic = {"user":self.get_user(user_id)["name"], "machine":machine["name"], "instructions":machine["instructions"]}
        hist = []
        for row in ret:
            hist.append({"data":row[3]})
        dic["zistory"] = hist
        return dic

    def get_next_activity(self, user_id):
        nb_type = self.get_type_nb()
        history = self.get_history(user_id)

        past = [0]*nb_type["retval"]

        for session in history["history"]:
            print(session)
            past[session["equip"]["type_id"] - 1] += 1

        #print(past)
        next = self.get_type_name(past.index(min(past))+1)

        return {"retval":next["retval"]}

    '''
    CREATE REQUESTS
    '''
    def create_user(self, id, name, age):
        req = "INSERT INTO users (id,name,age) VALUES ('{}', '{}', {})".format(id, name, age)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        return {"retval":"ok"}

    def create_equip(self, name, typ, instructions):
        req = "INSERT INTO equipement VALUES(NULL, '{}', {}, '{}')".format(name, typ, instructions)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        return {"retval":"ok"}

    def create_type(self, name):
        req = "INSERT INTO equip_type VALUES(NULL, '{}')".format(name)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        return {"retval":"ok"}

    def create_history(self, user_id, equip_id, data):
        req = "INSERT INTO history VALUES('{}',{},'{}','{}')".format(user_id, equip_id, str(datetime.now())[:19], data)
        ret = self.sql_request(req)
        if isinstance(ret, str):
            return {"error":"{}".format(ret)}
        return {"retval":"{}".format(self.get_next_activity(user_id)["retval"])}
        #return {"retval":"ok"}

    def __repr__(self):
        return "Sqlite3 database using file {}".format(self.file)
