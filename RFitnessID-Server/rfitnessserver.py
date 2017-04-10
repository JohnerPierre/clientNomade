import requests
import json
import sys
import os.path
import sport_db as db
from flask import Flask, jsonify, request
app = Flask(__name__)
app.secret_key = "k>FH-)T9VjL*3^Sjk2_[;\B(TBD/9DFc"

db_file = sys.argv[1]
bdd = db.Sport_db(db_file)

@app.route("/")
def test():
    return "historique : /db/get_history2/[tag_rfid]:[machine_id]<br/>Enregistrer : /db/create_history/[tag_rfid]:[machine_id]:[data]"

@app.route("/list/<table>")
def list_table(table):
    return jsonify(bdd.list_table(table))

@app.route("/db/get_user/<id>")
def get_user(id):
    return jsonify(bdd.get_user(id))

@app.route("/db/get_equip/<id>")
def get_equip(id):
    return jsonify(bdd.get_equip(id))

@app.route("/db/get_equip_type/<id>")
def get_equip_type(id):
    return jsonify(bdd.get_equip_type(id))

@app.route("/db/get_equip_type_nb/")
def get_equip_type_nb():
    return jsonify(bdd.get_type_nb())

@app.route("/db/get_history/<user_id>")
def get_history(user_id):
    return jsonify(bdd.get_history(user_id))

@app.route("/db/get_history2/<user_id>:<equip_id>")
def get_history2(user_id, equip_id):
    return jsonify(bdd.get_history2(user_id,equip_id))

@app.route("/db/get_next_activity/<user_id>")
def get_next_activity(user_id):
    return jsonify(bdd.get_next_activity(user_id))

@app.route("/db/create_user/<id>:<name>:<age>")
def create_user(id, name, age):
    return jsonify(bdd.create_user(id,name,age))

@app.route("/db/create_type/<name>")
def create_type(name):
    return jsonify(bdd.create_type(name))

@app.route("/db/create_history/<uid>:<eid>:<data>")
def create_history(uid,eid,data):
    return jsonify(bdd.create_history(uid,eid,data))

@app.route("/admin/create_user")
def adm_create_user():
    uid = request.args.get("user_id")
    uname = request.args.get("user_name")
    uage = request.args.get("user_age")
    retval = bdd.create_user(uid, uname, uage)
    return jsonify({"uid":uid, "uname":uname, "uage":uage, "sqlret":retval})

@app.route("/admin/create_equip")
def adm_create_equip():
    ename = request.args.get("equip_name")
    einstr = request.args.get("equip_instr")
    etype = int(request.args.get("etype"))
    retval = bdd.create_equip(ename, etype, einstr)
    return jsonify({"ename":ename, "edescr":einstr, "etype":etype, "sqlret":retval})

def main():
    app.run(host='0.0.0.0')
    #app.run(debug=True)

if __name__ == '__main__':
    main()
