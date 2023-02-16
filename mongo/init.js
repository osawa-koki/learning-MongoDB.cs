
db = db.getSiblingDB('my_database');

db.createUser({
  user: 'normal_user',
  pwd: 'Password1234',
  roles: [{ role: 'readWrite', db: 'my_database' }]
});
