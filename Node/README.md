# BotToBot-Handoff Sample Bot

A sample that shows how to create a single bot user experience that is made up of independently developed and deployed bots. 

In this sample, the user has a conversation with a single "main bot" that "hands-off" the conversation to various "sub-bots" for its functionality. Each sub-bot can be developed and deployed independently by disparate teams, even using different programming languages supported by the [Bot Framework](https://dev.botframework.com/).

For example, consider an "intranet bot" for a large enterprise that includes support for conversations about HR, Finance, HelpDesk, etc. Like an intranet site is made up of sub-sites (developed independently) for HR, Finance, HelpDesk, etc. and linked into a common navigation scheme, an "intranet bot" could provide navigation to different sub-bots for each function and hand-off conversations to the appropriate sub-bot.

### Prerequisites

To run this sample, install the prerequisites by following the steps in the [Bot Builder SDK for Node.js Quickstart](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-quickstart) section of the documentation.

This sample assumes you're familiar with:
* [Bot Builder for Node.js SDK](https://dev.botframework.com/)
* [Dialog API](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-dialog-overview)
* [Intercepting messages via middleware](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-intercept-messages)
* [Conversation Navigation](https://github.com/cmayomsft) (coming soon).

### Overview

In the bot-to-bot handoff scenario above, the user has a conversation with a bot that is actually comprised of two types of bots:
* Main Bot - A bot that provides conversational UI (CUI) to allow users navigate to the various sub-bots and hands off messages to the sub-bot.
* Sub-bots - Bots that provide the actual conversation functionality of the overall bot solution. Sub-bots provide their own navigation CUI for navigating their own conversation flows, CUI to support their conversation flows, and navigation CUI for navigating back to the main-bot.

### Conversation Navigation

In each of the bots, conversation navigation (or the ability to change the topic of conversation) is handled via [middleware](https://github.com/Microsoft/BotBuilder-Samples/tree/master/Node/capability-middlewareLogging) that inspect each message for pre-defined navigation commands supported by that bot. When middleware code matches a navigation command to the [`Text`](https://docs.botframework.com/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.imessage.html#text) of an [`IMessage`](https://docs.botframework.com/en-us/node/builder/chat-reference/interfaces/_botbuilder_d_.imessage.html), the handler changes the conversation flow by resetting the dialog stack and sending the message to the `RootDialog` for the bot. The `RootDialog` inspects the `IMessage` and calls the appropriate dialog for the conversation flow. Navigation commands are global to the bot, since every message is inspected for navigation commands, allowing the user to change the topic of conversation at any time in the conversation. Navigation commands differ from replies, which apply to a specific prompt ("What is your name?") from the active dialog in the conversation flow. A message is considered a reply to a prompt from the active dialog if it doesn't match any navigation commands. 

For more details on providing conversation navigation in a bot, see the [Conversation Navigation](https://github.com/cmayomsft) sample (coming soon).

### Handoff

In this sample, the handoff between the main bot each of the sub-bots is facilitated by the sub-bots sharing their messaging endpoint URL (e.g. /api/messages) and their top-level navigation commands (those that would cause a handoff to that sub-bot) with the main bot. 

The main bot inspects every incoming message. If the main bot matches the incoming message text with a sub-bot's top-level navigation command (similar to the [Conversation Navigation](#Conversation-Navigation) discussion above), the main bot forwards that message to the sub-bot's messaging URL. 

When the sub-bot receives that message (a navigation command), its middleware code takes the appropriate action by resetting the dialog stack, sending the message to its `RootDialog`, and calling the correct dialog to handle the navigation command.

The main bot will continue forwarding messages to that sub-bot until it encounters another navigation command for another bot.

Note: The main bot and each of the sub-bots share the same AppID and AppPassword. This allows all the bots to share the same conversation ID, [`Dialog Stack`](https://docs.botframework.com/en-us/node/builder/chat-reference/classes/_botbuilder_d_.session.html#dialogstack), and [Bot State Data](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-concepts#saving-state). 

### Bot-to-Bot Handoff in Action

Let's see the sample in action so you can see how the solution comes together.

The sample includes a main bot ([`MainBot`](\main-bot\index.js)), and two sub-bots ([`SubBot1`](\sub-bot-1\index.js) and [`SubBot2`](\sub-bot-2\index.js)), to show handing off from the main bot to sub-bots and back.

#### Navigation Commands

MainBot includes navigation commands to show its navigation menu and to hand-off to the sub-bots. SubBot1 and SubBot2 include navigation commands for navigating their conversation flows and for handing back to the MainBot, including:

* MainBot
  * "Menu" - Shows the navigation UI for the MainBot, in this sample a HeroCard with buttons that navigate, or hand-off, to each of the sub-bots.
  * "SubBot1" - Starts a hand-off to SubBot1 and displays the navigation UI for SubBot1.
  * "SubBot2" - Starts a hand-off to SubBot2 and displays the navigation UI for SubBot2.

* SubBot1
  * "SubBot1" - Shows the navigation UI for the SubBot1, in this sample a HeroCard with buttons that call dialogs for each of the SubBot1's conversation flows and a button to hand back to the MainBot.
  * "SubBot1.1" - Calls the SubBot1_1_Dialog dialog to start a conversation flow.
  * "SubBot1.2" - Calls the SubBot1_2_Dialog dialog to start a conversation flow.
  * "SubBot1.3" - Calls the SubBot1_3_Dialog dialog to start a conversation flow.
  * "Menu" - Shows the MainBot navigation UI, allowing the user to navigate to other sub bots.

* SubBot2
  * Follows the same pattern as the SubBot1 navigation above.

#### Sample Walkthrough

When you start the MainBot, type "Menu" to show the MainBot's navigation menu.

![MainBotMenu](images/mainbotmenu.png)

The MainBot navigation menu has buttons for navigating, or handing off, to each of the sub-bots, "SubBot1" and "SubBot2".

When you click on the "SubBot1" button, it posts "SubBot1" to the conversation. This will cause the SubBot1 bot to become the active bot. SubBot1 will respond by showing its navigation menu.

![SubBotMenu](images/subbot1menu.png)

**Note:** The HeroCard above is coming from SubBot1, not MainBot, but to the user it appears to be a single conversation with a single bot. 

The SubBot1 navigation menu has buttons for each of its navigation commands and to show the MainBot's navigation menu ("Menu").

Click the "SubBot1.1" button, which calls the `SubBot1_1_Dialog` to start a conversation flow.

![SubBot11Dialog](images/subbot11dialog.png)

At this point in the conversation, the user can reply with any of the navigation commands from any of the bots, or reply to the active prompt from the active dialog to move the conversation forward. In this case, clicking the "More" button will move the dialog to the next prompt. Replying with anything other than a navigation command or "More" will not be understood by the SubBot1_1_Dialog.

![SubBot11DialogReply](images/subbot11dialogreply.png)

When the SubBot1_1_Dialog completes, it shows a navigation menu with buttons that allow the user to navigate back to the SubBot1 navigation menu ("SubBot1"), or to hand back to the MainBot and show the main bot navigation menu ("Menu").

![SubBot11DialogDone](images/subbot11dialogdone.png)

Clicking the "SubBot1" button shows SubBot1's navigation menu.

![SubBot11DialogDoneSubBot1](images/subbot11dialogdonesubbot1.png)

Clicking the "Menu" button passes control back to MainBot and shows the MainBot's navigation menu.

![SubBot11DialogDoneMainBot](images/subbot11dialogdonemainbot.png)

Clicking the "SubBot2" button on the MainBot's navigation menu will hand off control to SubBot2 and show its navigation menu.

![MainBotMenuSubBot2](images/mainbotmenusubbot2.png)

**Note:** The user can type a navigation command (for example, "SubBot1") from anywhere in the conversation to hand-off to that bot.

![SubBot2BubBot1](images/subbot2subbot1.png)

### Code Walkthrough

Let's see how this is accomplished in code:

#### Managing Handoffs

In the MainBot's [`index.js`](\main-bot\index.js), MainBot stores the navigation commands that cause hand-offs to sub-bots and the corresponding sub-bot messaging URLs in an object. 

````node
// Dictionary of global commands to forwarding URLs
var commandUrls = {
    SubBot1: "http://localhost:3980/api/messages",
    SubBot2: "http://localhost:3981/api/messages",
    Menu: null
};
````

**Note:** These commands and URLs are hardcoded for simplicity. This could be done dynamically by adding a registration Web API to the MainBot so trusted bots could publish their navigation commands and forwarding URLs dynamically, for example. 

**Note:** Navigation commands must be unique across all the bots since the MainBot inspects every message, even those being forwarded to a sub-bot, for navigation commands.

The MainBot's [`index.js`](\main-bot\index.js) Post handler checks the type of the current event. If it is of type `message`, its `text` property is checked to see if it matches a navigation command for the main bot or one of the sub-bots. If there is a match, the messaging URL for the corresponding bot is saved to conversation state and will be used to hand-off the message to that bot.

````node
if (req.body.type === 'message') {
    var msgText = req.body.text;

    // If the message matches any global command, set the forwarding URL and clear the stack
    var matchedBot = Object.keys(commandUrls).find(val => val.toUpperCase() === msgText.toUpperCase());
    if (matchedBot !== undefined) {
        session.conversationData.forwardingUrl = commandUrls[matchedBot];
        session.clearDialogStack();
    }
}
````

Then, the forwarding URL for the current bot is retrieved from the conversation state. (This URL may have just been set in the previous block of code.)

````node
var currForwardingUrl = session.conversationData.forwardingUrl;
````

If the forwarding URL is valid, then it corresponds to a sub-bot, so the message is forwarded to the sub-bot by sending it to the URL.

````node
if (currForwardingUrl) {
    subBotHelper.send(currForwardingUrl, req.headers, req.body, res)
        .catch((err) => {
            // Sub-bot unreachable... stop forwarding and notify user
            ...
        });
}
````

The message is forwarded via an HTTP Post. See [`sub-bot-helper.js`](\main-bot\sub-bot-helper.js).

````node
...
var requestOpts = {
    uri: uri,
    headers: headers,
    json: true,
    body: body
};

// Update the host header to the sub-bot host name
requestOpts.headers.host = url.parse(requestOpts.uri).host;

// Forward request to sub-bot
request.post(requestOpts, (err, resp, body) => {
    ...
});
````

Otherwise, the message passes through to the MainBot.

````node
else {
    next();
}
````

#### Fielding a Handoff

In each "sub-bot", the middleware code will be inspecting incoming messages for navigation commands. Per the [Conversation Navigation](#Conversation-Navigation) discussion above, if the message text matches a navigation command, the dialog stack is cleared and the message is forwarded to the `Root Dialog`. 

In SubBot1's [`RootDialog`](\sub-bot-1\index.js), the navigation command is used to display the navigation menu card for the bot ("SubBot1") or to call the appropriate dialog for the command ("SubBot1.1").

Note: SubBot1 facilitates navigation back to the MainBot by adding a `CardAction` to its menu that posts the MainBot's main navigation command ("Menu") to the conversation. This navigation command will be picked up by the MainBot's [`POST handler`](\main-bot\index.js) and will cause the stack to be reset and the MainBot's navigation menu to be displayed.

````node
// Root dialog to sub bot's main menu
var rootDialog = [
    (session, result, next) => {
        var messageUpper = session.message.text.toUpperCase();

        if (messageUpper === res.SUB_BOT_1_1_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 1.1 Dialog');
        } else if (messageUpper === res.SUB_BOT_1_2_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 1.2 Dialog');
        } else if (messageUpper === res.SUB_BOT_1_3_MENU.toUpperCase()) {
            session.replaceDialog('SubBot 1.3 Dialog');
        } else if (messageUpper === res.SUB_BOT_1_MENU.toUpperCase()) {
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
                builder.CardAction.imBack(session, res.SUB_BOT_1_1_MENU, res.SUB_BOT_1_1_MENU),
                builder.CardAction.imBack(session, res.SUB_BOT_1_2_MENU, res.SUB_BOT_1_2_MENU),
                builder.CardAction.imBack(session, res.SUB_BOT_1_3_MENU, res.SUB_BOT_1_3_MENU),
                builder.CardAction.imBack(session, res.MAIN_BOT_MENU, res.MAIN_BOT_MENU)
            ]);

        var msg = new builder.Message(session).addAttachment(heroCard);
        session.send(msg);
    }
];
````


### More Information

For more information on the concepts shown in this sample, check out the following resources:
* [Bot Builder for Node.js SDK](https://dev.botframework.com/)
* [Dialog API](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-dialog-overview)
* [Intercepting messages via middleware](https://docs.microsoft.com/en-us/bot-framework/nodejs/bot-builder-nodejs-intercept-messages)
* [Conversation Navigation](https://github.com/cmayomsft) (coming soon).
