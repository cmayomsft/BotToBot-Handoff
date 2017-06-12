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

// Dictionary of global commands to forwarding URLs
var commandUrls = {
    [resources.SUB_BOT_1_MENU]: config.subBot1Url,
    [resources.SUB_BOT_2_MENU]: config.subBot2Url,
    [resources.MAIN_BOT_MENU]: null
};

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

                // If the message matches any global command, set the forwarding URL and clear the stack
                var matchedBot = Object.keys(commandUrls).find(val => val.toUpperCase() === msgText.toUpperCase());
                if (matchedBot !== undefined) {
                    session.conversationData.forwardingUrl = commandUrls[matchedBot];
                    session.clearDialogStack();
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
                            session.send(`Sorry, that sub-bot is unavailable right now. Sending your message to the main bot...`);
                            session.save().sendBatch(() => next());
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