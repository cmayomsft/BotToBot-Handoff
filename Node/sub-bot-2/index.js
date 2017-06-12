var restify = require('restify');
var builder = require('botbuilder');
var dialogBuilder = require('./dialog-builder');
var config = require('./config.json');
var res = require('./res');

// Setup restify server
var server = restify.createServer();
var serverPort = config.port || config.PORT || 3981;
server.listen(serverPort, () => {
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat connector for communicating with Bot Framework
var connector = new builder.ChatConnector({
    appId: config.appId,
    appPassword: config.appPassword
});

server.post('/api/messages', connector.listen());

// Root dialog to sub bot's main menu
var rootDialog = [
    (session, result, next) => {
        var messageUpper = session.message.text.toUpperCase();

        if (messageUpper === res.SUB_BOT_2_1_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 2.1 Dialog');
        } else if (messageUpper === res.SUB_BOT_2_2_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 2.2 Dialog');
        } else if (messageUpper === res.SUB_BOT_2_3_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 2.3 Dialog');
        } else if (messageUpper === res.SUB_BOT_2_MENU.toUpperCase()) {
            next();
        } else {
            // If we got here, it's because something other than a navigation command happened, and this dialog only supports navigation commands
            session.send(res.DO_NOT_UNDERSTAND);
            next();
        }
    },
    (session) => {
        var heroCard = new builder.HeroCard(session)
            .buttons([
                builder.CardAction.imBack(session, res.SUB_BOT_2_1_MENU, res.SUB_BOT_2_1_MENU),
                builder.CardAction.imBack(session, res.SUB_BOT_2_2_MENU, res.SUB_BOT_2_2_MENU),
                builder.CardAction.imBack(session, res.SUB_BOT_2_3_MENU, res.SUB_BOT_2_3_MENU),
                builder.CardAction.imBack(session, res.MAIN_BOT_MENU, res.MAIN_BOT_MENU)
            ]);

        var msg = new builder.Message(session).addAttachment(heroCard);
        session.send(msg);
    }
];

// Bot setup
var bot = new builder.UniversalBot(connector, rootDialog);
bot.dialog('SubBot 2.1 Dialog', dialogBuilder.build('SubBot 2.1 Dialog'));
bot.dialog('SubBot 2.2 Dialog', dialogBuilder.build('SubBot 2.2 Dialog'));
bot.dialog('SubBot 2.3 Dialog', dialogBuilder.build('SubBot 2.3 Dialog'));

// Middleware to intercept sub bot navigation commands
bot.use({
    botbuilder: (session, next) => {
        var msgText = session.message.text;

        // If it's a sub bot navigation command, force message to root dialog
        let navCommands = [res.SUB_BOT_2_1_MENU, res.SUB_BOT_2_2_MENU, res.SUB_BOT_2_3_MENU, res.SUB_BOT_2_MENU];
        if (navCommands.find(cmd => cmd.toUpperCase() === msgText.toUpperCase())) {
            session.clearDialogStack();
            session.save().sendBatch(() => next());
        } else {
            next();
        }
    }
});