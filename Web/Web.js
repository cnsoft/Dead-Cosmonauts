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

  Meteor.Router.add('/add/:playerId/:name', 'GET', function (playerId, name) {
    playerId = parseInt(playerId);
    if (Players.find({playerId: playerId}).count() > 0) {
      Players.update({playerId: playerId}, {$set: {name: name}});
    } else {
      Players.insert({playerId: playerId, name: name, x: 0.0, y: 0.0, rotationZ: 0.0, message: null});
    }

    return [200, defaultHeaders, Players.find({}).count().toString()];
  });

  Meteor.Router.add('/chat/:playerId/:message', 'GET',
    function (playerId, message) {
      playerId = parseInt(playerId);
      Players.update({playerId: playerId}, {$set: {message: message}});
      return [200, defaultHeaders, 1];
    });

  Meteor.Router.add('/update/:playerId/:x/:y/:rotationZ', 'GET',
    function (playerId, x, y, rotationZ, color) {
      var id = 0;
      playerId = parseInt(playerId);
      x = parseFloat(x);
      y = parseFloat(y);
      rotationZ = parseFloat(rotationZ);
      try {
        if (Players.find({playerId: playerId}).count() > 0) {
          Players.update({playerId: playerId},
            {$set: {x: y, y: x, rotationZ: rotationZ}});
        } else {
          id =
            Players.insert({playerId: playerId, x: y, y: x,
              rotationZ: rotationZ});
        }
      } catch (e) {
        return [500, '0'];
      }
      return [200, defaultHeaders, Players.find({}).count().toString()];
    });

  Meteor.Router.add('/powerups', 'GET', function () {
    var data = _.map(Powerups.find({delivered: false}).fetch(), function (p) {
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
    Powerups.remove({});
    return [200, defaultHeaders, '1'];
  });

  Meteor.Router.add('/delete/:id', 'GET', function (playerId) {
    playerId = parseInt(playerId);
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

  var powerupCounter = 0;
  var availablePowerups = [
    {type: 1, text: 'machinegun'},
    {type: 2, text: 'shotgun'},
    {type: 3, text: 'barricade'},
    {type: 4, text: '1x healthpack'}
//    {type: 5, text: 'autocannon'}
  ];

  var availablePowerup = null;

  var loadPowerup = function () {
    window.controlMessage.innerText = 'charging up';
    window.progressBar.className =
      'controlpanel-progress controlpanel-progress-load';
    Meteor.setTimeout(function () {
      availablePowerup =
        availablePowerups[powerupCounter % availablePowerups.length];
      window.controlMessage.innerText = availablePowerup.text;
      powerupCounter ++;
      window.tutorialMessage.style.visibility = 'visible';
    }, 5000);
  };

  var playerIcon = L.icon({
    iconUrl: '/pin.png',
    iconSize: [64, 64],
    iconAnchor: [32, 32],
    labelAnchor: [16, - 16]
  });

  var powerupIcon = L.icon({
    iconUrl: '/crate.png',
    iconSize: [32, 32],
    iconAnchor: [16, 16],
    labelAnchor: [16, - 16]
  });

  var medpackIcon = L.icon({
    iconUrl: '/health.png',
    iconSize: [32, 32],
    iconAnchor: [16, 16],
    labelAnchor: [16, - 16]
  });

  var brickIcons = _.shuffle([
    L.icon({
      iconUrl: '/brick0.png',
      iconSize: [32, 32],
      iconAnchor: [16, 16],
      labelAnchor: [16, - 16]
    }),
    L.icon({
      iconUrl: '/brick1.png',
      iconSize: [32, 32],
      iconAnchor: [16, 16],
      labelAnchor: [16, - 16]
    }),
    L.icon({
      iconUrl: '/brick2.png',
      iconSize: [32, 32],
      iconAnchor: [16, 16],
      labelAnchor: [16, - 16]
    })
  ]);

  Template.mapTemplate.rendered = function () {
    loadPowerup();

    L.Icon.Default.imagePath = 'packages/leaflet/images';

    window.map = L.map('map',
      {doubleClickZoom: false, zoomControl: false, attributionControl: false}).setMaxBounds(bounds).fitBounds(bounds);
    L.imageOverlay('/map.jpg', bounds).addTo(window.map);

    window.map.on('dblclick', function (e) {
      if (availablePowerup) {
        var lt = e.latlng;
        Powerups.insert({x: lt.lat, y: lt.lng, type: availablePowerup.type, delivered: false});
        availablePowerup = null;
        window.progressBar.className = 'controlpanel-progress';
        window.tutorialMessage.style.visibility = 'hidden';
        Meteor.defer(function () {
          loadPowerup();
        });
      }
    });

    var playersObserve = Players.find({}).observe({
      added: function (player) {
        markers[player._id] =
          new L.Marker([player.x, player.y],
            {icon: playerIcon}).bindLabel(player.name, {noHide: true});
        window.map.addLayer(markers[player._id]);
        markers[player._id].showLabel();
      },
      removed: function (player) {
        window.map.removeLayer(markers[player._id]);
      },
      changed: function (player) {
        var lat = (player.x);
        var lng = (player.y);
        var newLatLng = new L.LatLng(lat, lng);
        markers[player._id].setLatLng(newLatLng);
        if (player.message != null) {
          markers[player._id].updateLabelContent(player.name + ": " +
            player.message);
        }
      }
    });

    var powerupsObserve = Powerups.find({}).observe({
      added: function (powerup) {
        var icon = powerupIcon;
        if (powerup.type == 3) {
          icon = _.first(_.shuffle(brickIcons));
        } else if (powerup.type == 4) {
          icon = medpackIcon;
        }
        markers[powerup._id] =
          new L.Marker([powerup.x, powerup.y], {icon: icon});
        icon = null;
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