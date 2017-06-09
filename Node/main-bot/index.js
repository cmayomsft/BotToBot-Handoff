var restify = require('restify');
var builder = require('botbuilder');
var subBotHelper = require('./sub-bot-helper');
var config = require('./config.json');
var resources = require('./resources');

// Setup restify server
var server = restify.createServer();
server.use(restify.bodyParser({
    mapParams: false
}));
server.listen(config.port || config.PORT || 3978, () => {
    console.log('%s listening to %s', server.name, server.url);
});

// Create chat connector for communicating with Bot Framework
var connector = new builder.ChatConnector({
    appId: config.appId,
    appPassword: config.appPassword
});

// Intercepts and routes all messages
server.post('/api/messages', [
    (req, res, next) => {
        if (!req.body) {
            return next();
        }

        var address = {
            id: req.body.id,
            channelId: req.body.channelId,
            user: req.body.from,
            conversation: req.body.conversation,
            bot: req.body.recipient,
            serviceUrl: req.body.serviceUrl
        };

        bot.loadSession(address, (err, session) => {
            if (err) {
                return next();
            }

            if (req.body.type === 'message') {
                var msgText = req.body.text;

                // Check for main bot commands
                if (msgText.toUpperCase() === resources.MAIN_BOT_MENU.toUpperCase()) {
                    session.conversationData.forwardingUrl = null;
                    session.clearDialogStack();
                } else {
                    // Check for sub bot commands
                    var subBot = Object.keys(commandUrls).find(val => val.toUpperCase() === msgText.toUpperCase());
                    if (subBot && session.conversationData.forwardingUrl !== commandUrls[subBot]) {
                        // Prepare to route messages to this sub bot
                        session.conversationData.forwardingUrl = commandUrls[subBot];
                        session.clearDialogStack();
                    }
                }
            }

            // Ensure state (including stack) is saved before continuing
            session.save().sendBatch(() => {
                // Route to sub-bot, if appropriate
                // otherwise, call next() to continue with main bot
                var currForwardingUrl = session.conversationData.forwardingUrl;
                if (currForwardingUrl) {
                    subBotHelper.send(currForwardingUrl, req.headers, req.body, res)
                        .catch((err) => {
                            // Sub-bot unreachable... stop forwarding and notify user
                            session.conversationData.forwardingUrl = null;
                            session.clearDialogStack();
                            session.send(`Sorry, that bot is unavailable right now. I'm sending you back to the main bot.`);
                            session.save().sendBatch(() => next()); // Ensure state (including stack) is saved before continuing
                        });
                } else {
                    next();
                }
            });
        });
    },
    connector.listen()
]);

// Root dialog to show main bot's menu
var rootDialog = [
    (session, args, next) => {
        var msgText = session.message.text;
        if (msgText.toUpperCase() !== resources.MAIN_BOT_MENU.toUpperCase()) {
            // If we got here, it's because something other than a navigation command happened, and this dialog only supports navigation commands
            session.send(resources.DO_NOT_UNDERSTAND);
        }

        // Send the main menu
        var card = new builder.HeroCard(session)
            .buttons([
                builder.CardAction.imBack(session, resources.SUB_BOT_1_MENU, resources.SUB_BOT_1_MENU),
                builder.CardAction.imBack(session, resources.SUB_BOT_2_MENU, resources.SUB_BOT_2_MENU)
            ]);

        var msg = new builder.Message(session).addAttachment(card);
        session.send(msg);
    }
];

// Bot setup
var bot = new builder.UniversalBot(connector, rootDialog)
    .set('persistConversationData', true);

var commandUrls = {
    [resources.SUB_BOT_1_MENU]: config.subBot1Url,
    [resources.SUB_BOT_2_MENU]: config.subBot2Url
};