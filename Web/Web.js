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
      playerId = parseInt(playerId);
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
    var data = _.map(Powerups.find({delivered: false}).fetch(), function (p) {
      p.x -= origin[0];
      p.y -= origin[1];
      p.x /= scale;
      p.y /= scale;
      p.y *= - 1;
      return p;
    });

    Powerups.update({delivered: false}, {$set: {delivered: true}},
      {multi: true});

    return [200, JSON.stringify({success: true, data: data})];
  });

  Meteor.Router.add('/powerups/pickup/:powerupId',function(powerupId) {
    Powerups.remove({_id:powerupId});
  });

  Meteor.Router.add('/clear', 'GET', function () {
    Players.remove({});
    return [200, '1'];
  });

  Meteor.Router.add('/delete/:id', 'GET', function (playerId) {
    Players.remove({playerId: playerId}, {multi: true});
  });
}

var scale = 10;
var origin = [160, 240];

if (Meteor.isClient) {

  Template.radarTemplate.events = {
    'tap, click #container': function (e) {
      Powerups.insert({x: e.pageX, y: e.pageY, type: 2, delivered: false});
    }
  };

  Template.radarTemplate.players = function () {
    return _.map(Players.find({}).fetch(), function (p) {
      p.y *= - 1;
      p.y *= scale;
      p.x *= scale;
      p.x += origin[0];
      p.y += origin[1];
      return p;
    });
  };

  Template.radarTemplate.powerups = function () {
    return Powerups.find({}).fetch();
  }

  Template.radarTemplate.preserve = defaultPreserve;
}