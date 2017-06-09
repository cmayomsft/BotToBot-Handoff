var builder = require('botbuilder');
var res = require('./res');

// Creates an entire waterfall dialog
function buildSubBotDialog(name) {
    return [
        buildSubBotWaterfallStep(`${name} dialog text...`, [res.MORE_REPLY]),
        buildSubBotWaterfallStep(`${name} second dialog text...`, [res.MORE_REPLY]),
        buildSubBotWaterfallStep(`${name} third dialog text...`, [res.MORE_REPLY]),
        buildSubBotWaterfallStep(`${name} is done. What do you want to do next?...`, [res.SUB_BOT_1_MENU, res.MAIN_BOT_MENU]),

        (session, result) => {
            // If we got here, it's because something other than a navigation command happened, and this dialog only supports navigation commands
            session.send(res.DO_NOT_UNDERSTAND);
            session.replaceDialog(name);
        }
    ];
}

// Creates a single waterfall step
function buildSubBotWaterfallStep(msgText, choices) {
    return (session, result) => {
        builder.Prompts.choice(session, msgText, choices, {
            retryPrompt: res.DO_NOT_UNDERSTAND,
            listStyle: builder.ListStyle.button
        });
    };
}

module.exports.build = buildSubBotDialog;