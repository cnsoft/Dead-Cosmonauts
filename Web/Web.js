Players = new Meteor.Collection("players");
Powerups = new Meteor.Collection("powerups");

if (Meteor.isServer) {
  var defaultHeaders = {
    "Access-Control-Allow-Origin": "*"
  };

  Meteor.startup(function () {
    Players.remove({});
    Powerups.remove({});
  });

  Meteor.Router.add('/update/:playerId/:x/:y/:rotationZ/:color', 'GET',
    function (playerId, x, y, rotationZ, color) {
      var id = 0;
      playerId = parseInt(playerId);
      x = parseFloat(x);
      y = parseFloat(y);
      rotationZ = parseFloat(rotationZ);
      try {
        if (Players.find({playerId: playerId}).count() > 0) {
          Players.update({playerId: playerId},
            {$set: {x: y, y: x, rotationZ: rotationZ, color: color}});
        } else {
          id =
            Players.insert({playerId: playerId, x: y, y: x,
              rotationZ: rotationZ, color: color});
        }
      } catch (e) {
        return [500, '0'];
      }
      return [200, defaultHeaders, Players.find({}).count().toString()];
    });

  Meteor.Router.add('/powerups', 'GET', function () {
    var data = _.map(Powerups.find({delivered: false}).fetch(),function(p){
      var _y = p.y;
      p.y = p.x;
      p.x = _y;
      return p;
    });

    Powerups.update({delivered: false}, {$set: {delivered: true}},
      {multi: true});

    return [200, defaultHeaders, JSON.stringify({success: true, data: data})];
  });

  Meteor.Router.add('/powerups/pickup/:powerupId', function (powerupId) {
    Powerups.remove({_id: powerupId});
    return [200, defaultHeaders, '1'];
  });

  Meteor.Router.add('/clear', 'GET', function () {
    Players.remove({});
    return [200, defaultHeaders, '1'];
  });

  Meteor.Router.add('/delete/:id', 'GET', function (playerId) {
    Players.remove({playerId: playerId}, {multi: true});
    return [200, defaultHeaders, '1'];
  });
}

var bounds = [
  [- 30, - 40],
  [30, 40]
];

if (Meteor.isClient) {
  var markers = {};

  var playerIcon = L.icon({
    iconUrl: '/pin.png',
    iconSize: [64, 64],
    iconAnchor: [32, 32],
    popupAnchor: [0, - 16]
  });

  var powerupIcon = L.icon({
    iconUrl: '/crate.png',
    iconSize: [32, 32],
    iconAnchor: [16, 16],
    popupAnchor: [0, - 8]
  });

  Template.mapTemplate.rendered = function () {
    L.Icon.Default.imagePath = 'packages/leaflet/images';

    window.map = L.map('map',
      {doubleClickZoom: false, attributionControl: false}).setMaxBounds(bounds).fitBounds(bounds);
    L.imageOverlay('/map.jpg', bounds).addTo(window.map);

    window.map.on('dblclick', function (e) {
      var lt = e.latlng;
      Powerups.insert({x: lt.lat, y: lt.lng, type: 2, delivered: false});
    });

    var playersObserve = Players.find({}).observe({
      added: function (player) {
        markers[player._id] =
          new L.Marker([player.x, player.y], {icon: playerIcon});
        window.map.addLayer(markers[player._id]);
      },
      removed: function (player) {
        window.map.removeLayer(markers[player._id]);
      },
      changed: function (player) {
        var lat = (player.x);
        var lng = (player.y);
        var newLatLng = new L.LatLng(lat, lng);
        markers[player._id].setLatLng(newLatLng);
      }
    });

    var powerupsObserve = Powerups.find({}).observe({
      added: function (powerup) {
        markers[powerup._id] =
          new L.Marker([powerup.x, powerup.y], {icon: powerupIcon});
        window.map.addLayer(markers[powerup._id]);
      },
      removed: function (powerup) {
        window.map.removeLayer(markers[powerup._id]);
      },
      changed: function (powerup) {
        var lat = (powerup.x);
        var lng = (powerup.y);
        var newLatLng = new L.LatLng(lat, lng);
        markers[powerup._id].setLatLng(newLatLng);
      }
    });
  };
}