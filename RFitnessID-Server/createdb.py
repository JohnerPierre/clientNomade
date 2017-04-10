import sport_db as db
import sys
db.create_sport_db(sys.argv[1])
print("{} created".format(sys.argv[1]))
