Players = new Meteor.Collection("players");

defaultPreserve = {
  'div[id]': function (node) {
    return node.id;
  }
};

if (Meteor.isServer) {
  Meteor.Router.add('/update/:playerId/:x/:y/:rotationZ/:color','GET',function(playerId,x,y,rotationZ,color) {
    var id = 0;
    try {
      if (Players.find({playerId:playerId}).count() > 0) {
        Players.update({playerId:playerId},{$set:{x:x,y:y,rotationZ:rotationZ,color:color}});
      } else {
        id = Players.insert({playerId:playerId,x:x,y:y,rotationZ:rotationZ,color:color});
      }
    } catch (e) {
      return [500,'0'];
    }
    return [200,Players.find({}).count().toString()];
  });

  Meteor.Router.add('/list','GET',function() {
    return [200,JSON.stringify({success:true,data:Players.find({}).fetch()})];
  });

  Meteor.Router.add('/clear','GET',function() {
    Players.remove({});
    return [200,'1'];
  });
}

if (Meteor.isClient) {
  Template.radarTemplate.players = function () {
    return Players.find({}).fetch();
  }

  Template.radarTemplate.preserve = defaultPreserve;
}