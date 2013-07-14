Players = new Meteor.Collection("players");
Powerups = new Meteor.Collection("powerups");

defaultPreserve = {
  'div[id]': function (node) {
    return node.id;
  }
};

if (Meteor.isServer) {
  Meteor.Router.add('/update/:playerId/:x/:y/:rotationZ/:color', 'GET',
    function (playerId, x, y, rotationZ, color) {
      var id = 0;
      try {
        if (Players.find({playerId: playerId}).count() > 0) {
          Players.update({playerId: playerId},
            {$set: {x: x, y: y, rotationZ: rotationZ, color: color}});
        } else {
          id =
            Players.insert({playerId: playerId, x: x, y: y,
              rotationZ: rotationZ, color: color});
        }
      } catch (e) {
        return [500, '0'];
      }
      return [200, Players.find({}).count().toString()];
    });

  Meteor.Router.add('/powerups', 'GET', function () {
    var data = Powerups.find({}).fetch();
    Powerups.remove({});
    return [200, JSON.stringify({success: true, data: data})];
  });

  Meteor.Router.add('/clear', 'GET', function () {
    Players.remove({});
    return [200, '1'];
  });

  Meteor.Router.add('/delete/:id', 'GET', function () {
    Players.remove({playerId: playerId}, {multi: true});
  });
}

var scale = 10;
var origin = [160, 240];

if (Meteor.isClient) {

  Template.radarTemplate.events = {
    'tap, click #container': function (e) {
      Powerups.insert({x: (e.pageX - origin[0]) / scale,
        y: (e.pageY - origin[1]) / -scale, type: 2});
    }
  };

  Template.radarTemplate.players = function () {
    return _.map(Players.find({}).fetch(), function (p) {
      p.y *= - 1;
      p.y *= scale;
      p.x *= scale;
      p.x += origin[0];
      p.y += origin[0];
      return p;
    });
  };

  Template.radarTemplate.preserve = defaultPreserve;
}