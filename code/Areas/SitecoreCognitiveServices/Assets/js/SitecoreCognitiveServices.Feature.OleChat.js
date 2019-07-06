jQuery.noConflict();

//ole chat
var chatInput = ".chat-input";
var chatForm = ".chat-form";
var chatSubmit = ".chat-submit";
var chatConversation = ".chat-conversation";
var initText = ".init-text";
var chatMicrophoneButton = ".microphone-button";
var chatVoice = ".chat-voice";
var audioToggle = chatForm + " .audio-toggle";
var audioPlayer = "#audio-player";
var audioPlayerSource = audioPlayer + " source";
var enabled = "enabled";
var isAudioEnabled = false;
var itemSearchForm = ".item-search-form";
var itemSearchClose = ".item-search-header img";
var itemSearchInput = ".item-search-input";
var itemSearchResults = ".item-search-results";
var itemSearchTitle = ".item-search-title";
var itemSearchTimer;
var listSearchForm = ".list-search-form";
var listSearchClose = ".list-search-header img";
var listSearchInput = ".list-search-input";
var listSearchResults = ".list-search-results";
var listSearchOptions = ".list-search-options";
var listSearchTimer;

jQuery(document).ready(function ()
{
    var hasSpeechSDK = !!window.SpeechSDK;
    var SpeechSDK = hasSpeechSDK ? window.SpeechSDK : null;
    if (hasSpeechSDK)
        EnableMicButton();

    //initiate conversation
    SendChatRequest(jQuery(initText).text());

    jQuery(chatInput).focus();

    //chat microphone
    jQuery(chatForm + " " + chatMicrophoneButton).click(function (event) {
        event.preventDefault();
        DisableMicButton();
        jQuery(chatInput).val("");

        jQuery
            .post(jQuery(chatForm).attr("token-action"), {})
            .done(function (r) {
                var speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(r.Token, r.Region);
                speechConfig.speechRecognitionLanguage = r.Language;
                var audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();
                var recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);

                recognizer.recognizeOnceAsync(
                    function (result) {
                        EnableMicButton();
                        if (result.text === undefined)
                            return;

                        if (result.text !== "") {
                            var adjustedText = result.text.toLowerCase().split("slash").join("/").replace(/.\s*$/, "");

                            UpdateChatWindow(adjustedText, null, "user");
                            SendChatRequest(adjustedText);
                        }

                        recognizer.close();
                        recognizer = undefined;
                    },
                    function (err) {
                        EnableMicButton();
                        window.console.log({ err });

                        recognizer.close();
                        recognizer = undefined;
                    });
            });
    });

    //item search microphone
    jQuery(itemSearchForm + " " + chatMicrophoneButton).click(function (event)
    {
        event.preventDefault();
        DisableMicButton();
        jQuery(itemSearchInput).val("");

        jQuery
            .post(jQuery(chatForm).attr("token-action"), {})
            .done(function (r) {
                var speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(r.Token, r.Region);
                speechConfig.speechRecognitionLanguage = r.Language;
                var audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();
                var recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);

                recognizer.recognizeOnceAsync(
                    function (result) {
                        EnableMicButton();
                        if (result.text === undefined)
                            return;

                        if (result.text !== "") {
                            var adjustedText = result.text.toLowerCase().split("slash").join("/").replace(/.\s*$/, "");

                            SpellCheckAndSet(adjustedText, itemSearchInput);
                        }

                        recognizer.close();
                        recognizer = undefined;
                    },
                    function (err) {
                        EnableMicButton();
                        window.console.log({ err });

                        recognizer.close();
                        recognizer = undefined;
                    });
            });
    });

    //list search microphone
    jQuery(listSearchForm + " " + chatMicrophoneButton).click(function (event) {
        event.preventDefault();
        DisableMicButton();
        jQuery(listSearchInput).val("");

        jQuery
            .post(jQuery(chatForm).attr("token-action"), {})
            .done(function (r) {
                var speechConfig = SpeechSDK.SpeechConfig.fromAuthorizationToken(r.Token, r.Region);
                speechConfig.speechRecognitionLanguage = r.Language;
                var audioConfig = SpeechSDK.AudioConfig.fromDefaultMicrophoneInput();
                var recognizer = new SpeechSDK.SpeechRecognizer(speechConfig, audioConfig);

                recognizer.recognizeOnceAsync(
                    function (result) {
                        EnableMicButton();
                        if (result.text === undefined)
                            return;

                        if (result.text !== "") {
                            var adjustedText = result.text.toLowerCase().split("slash").join("/").replace(/.\s*$/, "");

                            SpellCheckAndSet(adjustedText, listSearchInput);
                        }

                        recognizer.close();
                        recognizer = undefined;
                    },
                    function (err) {
                        EnableMicButton();
                        window.console.log({ err });

                        recognizer.close();
                        recognizer = undefined;
                    });
            });
    });

    //enable and disable audio playback
    jQuery(audioToggle).click(function (event) {
        event.preventDefault();

        if (jQuery(this).hasClass(enabled)) {
            jQuery(this).removeClass(enabled);
            isAudioEnabled = false;
        }
        else {
            jQuery(this).addClass(enabled);
            isAudioEnabled = true;
        }
    });
    
    //sends text on click on the chat form
    jQuery(chatSubmit).click(function (event)
    {
        event.preventDefault();
        var queryValue = jQuery(chatInput).val();
        if (queryValue === "")
            return;

        jQuery(chatInput).val("");
        jQuery(chatInput).attr("type", "text");
        UpdateChatWindow(queryValue, null, "user");

        SendChatRequest(queryValue);
    });
    
    jQuery(itemSearchInput).keyup(function (e) {
        clearTimeout(itemSearchTimer);

        itemSearchTimer = setTimeout(function () { SendItemSearchRequest(jQuery(itemSearchInput).val()); }, 200);
    });

    jQuery(itemSearchClose).click(function (e)
    {
        SendChatRequest("quit");
        jQuery(itemSearchForm).hide();
        jQuery(itemSearchResults).html("");
        jQuery(itemSearchInput).val("");
    });

    //sends text on 'enter-press' on the list search form
    jQuery(listSearchInput).keyup(function (e)
    {
        clearTimeout(listSearchTimer);

        listSearchTimer = setTimeout(function () { SearchList(jQuery(listSearchInput).val()); }, 100);
    });

    jQuery(listSearchClose).click(function (e) {
        SendChatRequest("quit");
        jQuery(listSearchForm).hide();
        jQuery(listSearchResults).html("");
        jQuery(listSearchInput).val("");
    });

    function SendItemSearchRequest(queryValue)
    {
        jQuery(itemSearchResults).html("");

        if (queryValue === "")
            return;
        
        var langValue = jQuery(".item-search-lang").val();
        var dbValue = jQuery(".item-search-db").val();
        
        jQuery
            .post(jQuery(itemSearchForm).attr("action"),
            {
                db: dbValue,
                language: langValue,
                query: queryValue
            })
            .done(function (r) {
                HandleItemSearchResults(r);
            });
    }
    
    function SearchList(queryValue)
    {
        var options = jQuery(listSearchOptions).val().split("|");

        if (options.length < 1) {
            jQuery(itemSearchResults).html("<div class='no-results'>No Results</div>");
            return;
        }

        var resultHtml = "";
        for (var d = 0; d < options.length; d++)
        {
            if (queryValue !== "" && options[d].indexOf(queryValue) === -1)
                continue;

            resultHtml += "<div class='result-item'>";
            resultHtml += "<div class='title'>" + options[d] + "</div>";
            resultHtml += "</div>";
        }

        jQuery(listSearchResults).html(resultHtml);

        jQuery(listSearchResults + " .result-item")
            .on('click', function () {
                var titleValue = jQuery(this).find(".title").text();
                UpdateChatWindow(titleValue, null, "user");
                SendChatRequest(titleValue);
                jQuery(listSearchForm).hide();
                jQuery(listSearchResults).html("");
                jQuery(listSearchInput).val("");
            });
    }

    function HandleItemSearchResults(results)
    {
        if (results.Items.length < 1)
        {
            jQuery(itemSearchResults).html("<div class='no-results'>No Results</div>");
            return;
        }

        var resultHtml = "";
        for (var d = 0; d < results.Items.length; d++)
        {
            var r = results.Items[d];
            resultHtml += "<div class='result-item'>";
            resultHtml += "<div class='title'>" + r.Icon + r.Title + "</div>";
            resultHtml += "<div class='path'>" + r.Path + "</div>";
            resultHtml += "</div>";
        }

        jQuery(itemSearchResults).html(resultHtml);

        jQuery(itemSearchResults + " .result-item")
            .on('click', function () {
                var pathValue = jQuery(this).find(".path").text();
                UpdateChatWindow(pathValue, null, "user");
                SendChatRequest(pathValue);
                jQuery(itemSearchForm).hide();
                jQuery(itemSearchResults).html("");
                jQuery(itemSearchInput).val("");
            });
    }

    function SpellCheckAndSet(textValue, specifier)
    {
        jQuery
            .post(jQuery(chatForm).attr("spell-check-action"),
                {
                    text: textValue
                })
            .done(function (r) {
                jQuery(specifier).val(r.text).keyup();
            });
    }

    function SendChatRequest(queryValue)
    {
        var langValue = jQuery(".chat-lang").val();
        var dbValue = jQuery(".chat-db").val();
        var idValue = jQuery(".chat-id").val();

        jQuery(".message ul").removeClass("enabled").addClass("disabled");
        jQuery(".message .user-option, .message .user-selection, .message .option-submit").off("click");
        jQuery(".message .confirm-continue, .message .confirm-cancel, .message input, .message .option-submit").remove();
        
        jQuery
            .post(jQuery(chatForm).attr("action"), GenerateActivity(queryValue, langValue, dbValue, idValue))
            .done(function (r) {
                UpdateChatWindow(r.Text, r.ChannelData, "bot");
            })
            .fail(function (xhr, status, error) {
                var statusMsg = (xhr.status !== null) 
                    ? xhr.status + ":" + error
                    : error;

                var troubleText = jQuery(".trouble-message").text();
                UpdateChatWindow(troubleText + "...<br/><br/>" + statusMsg, null, "bot");
            });
    }

    function UpdateChatWindow(text, channelData, userType)
    {
        var convoBox = jQuery(chatConversation);
        convoBox.append("<div class='" + userType + "'><span class='message'>" + text + "<span class='icon'></span></span></div>");

        if (channelData !== null) {
            //options
            var formInput = channelData.Input;
            if (formInput !== null)
            {
                var hasOptions = formInput.Options !== null && formInput.Options.length > 0;
                if (formInput.InputType === "LinkList" && hasOptions)
                    SetupLinkList(userType, formInput.Options);
                else if (formInput.InputType === "Password")
                    SetupPasswordInput();
                else if (formInput.InputType === "ItemSearch")
                    SetupItemSearch(formInput.InputLabel);
                else if (formInput.InputType === "Radio")
                    SetupRadioList(userType, formInput.Options);
                else if (formInput.InputType === "Checkbox" && hasOptions)
                    SetupCheckboxList();
                else if (formInput.InputType === "ListSearch" && hasOptions)
                    SetupListSearch(formInput.Options);
            }
            
            //actions
            var action = channelData.Action;
            var selections = channelData.Selections;
            if (action !== null) {
                if (action === "logout")
                    HandleLogout();
                else if (action === "confirm")
                    HandleConfirm(userType, selections);
            }

            if (isAudioEnabled)
                PlayAudioFile(channelData.AudioFile);            
        }
        
        convoBox.scrollTop(convoBox[0].scrollHeight - convoBox.height());
    }

    function SetupLinkList(userType, options)
    {
        var optionList = "";
        for (i = 0; i < options.length; i++) {
            optionList += "<li class='user-option' data-option='" + options[i].Value + "'>" + options[i].Text + "</li>";
        }
        jQuery(chatConversation).append("<div class='" + userType + " option-list'><span class='message'><ul class='enabled'>" + optionList + "</ul><span class='icon'></span></span></div>");

        jQuery(".enabled .user-option")
            .on('click', function () {
                var optionValue = jQuery(this).text();
                UpdateChatWindow(optionValue, null, "user");
                SendChatRequest(optionValue);
            });
    }

    function SetupPasswordInput()
    {
        jQuery(searchInput).attr("type", "password");
    }

    function SetupItemSearch(inputLabel)
    {
        jQuery(itemSearchForm).show();
        jQuery(itemSearchTitle).text(inputLabel);
        jQuery(itemSearchInput).focus();
    }

    function SetupRadioList(userType, options)
    {
    }

    function SetupCheckboxList(userType, options)
    {
        var optionList = "";
        for (i = 0; i < options.length; i++) {
            optionList += "<li class='user-checkbox-option'>";
            optionList += "<input type='checkbox' id='userOption-" + options[i].Value + "' name='userOption' value='" + options[i].Value + "' />";
            optionList += "<label for='userOption-" + options[i].Value + "'>" + options[i].Text + "</label>";
            optionList += "</li>";
        }
        jQuery(chatConversation).append("<div class='" + userType + " option-list'><span class='message'><ul class='checkbox-list enabled'>" + optionList + "<span class='option-submit'>Submit</span></ul><span class='icon'></span></span></div>");

        jQuery(".enabled .user-checkbox-option")
            .on('click', function () {
                jQuery(this).removeClass("checked");
                if (jQuery(this).find("input").is(':checked'))
                    jQuery(this).addClass("checked");
            });

        jQuery(".enabled .option-submit")
            .on("click", function () {
                var checkedItems = jQuery(this).parent().find(".user-checkbox-option input:checked");
                if (checkedItems.length === 0)
                    return;

                var checkedLabels = jQuery(this).parent().find(".user-checkbox-option.checked label");
                var optionValues = [];
                var optionNames = [];
                checkedItems.each(function () {
                    optionValues.push(jQuery(this).val());
                });
                checkedLabels.each(function () {
                    optionNames.push(jQuery(this).text());
                });
                UpdateChatWindow(optionNames.join(", "), null, "user");
                SendChatRequest(optionValues.join("|"));
            });
    }

    function SetupListSearch(options)
    {
        var data = [];
        for (var i = 0; i < options.length; i++)
        {
            data.push(options[i].Text);
        }
        jQuery(listSearchOptions).val(data.join("|"));
        SearchList("");
        jQuery(listSearchForm).show();
        jQuery(listSearchInput).focus();
    }

    function HandleLogout()
    {
        var formData = {};
        formData.__RequestVerificationToken = jQuery("input[name=__RequestVerificationToken]").val();

        jQuery
            .post("/sitecore/shell/api/sitecore/Authentication/Logout?sc_database=master", formData)
            .done(function (msg) {
                location.href = "/sitecore/login";
                console.log("succeeded");
            })
            .fail(function () {
                console.log("error");
            });
    }

    function HandleConfirm(userType, selections)
    {
        var clearText = jQuery(".clear-message").text();
        var continueText = jQuery(".continue-message").text();
        var cancelText = jQuery(".cancel-message").text();

        var selectionList = "";
        for (var i in selections) {
            var displayValue = (i === "builtin.datetimeV2.datetime") ? "Date" : i;
            selectionList += "<li class='user-selection' data-selection='" + clearText + " " + i + "'><span class='field-name'>" + displayValue + ": </span><span class='field-value'>" + selections[i] + "</span></li>";
        }
        selectionList += "<div class='user-selection confirm-continue' data-selection='" + continueText + "'>" + continueText + "</div>";
        selectionList += "<div class='user-selection confirm-cancel' data-selection='" + cancelText + "'>" + cancelText + "</div>";
        jQuery(chatConversation).append("<div class='" + userType + " option-list'><span class='message'><ul class='enabled'>" + selectionList + "</ul><span class='icon'></span></span></div>");

        //click to clear a selection
        jQuery(".enabled .user-selection")
            .on('click', function () {
                var selectionValue = jQuery(this).data("selection");
                UpdateChatWindow(selectionValue, null, "user");
                SendChatRequest(selectionValue);
            });
    }

    function PlayAudioFile(audioFile)
    {
        if (audioFile === null || audioFile.length < 1)
            return;

        var audio = jQuery(audioPlayer);   
        if (jQuery(audioPlayerSource).attr("src").length > 0)
            audio[0].pause();
        
        jQuery(audioPlayerSource).attr("src", audioFile);
        try {
            audio[0].load();
            audio[0].oncanplaythrough = audio[0].play();
            audio[0].play();
        } catch (error) {
            console.log(error);
        }
    }
    
    function GenerateActivity(query, langValue, dbValue, idValue) {

        var data = {
            language: langValue,
            database: dbValue,
            id: idValue
        };

        var activity = {
            type: "message",
            id: GenerateGuid(),
            timestamp: Date.now(),
            channelId: "ole",
            from: {
                id: "2c1c7fa3",
                name: "User1"
            },
            conversation: {
                isGroup: false,
                id: "8a684db8",
                name: "Conv1"
            },
            recipient: {
                id: "56800324",
                name: "Bot1"
            },
            text: query,
            attachments: [],
            entities: [],
            channelData: JSON.stringify(data)
        }

        return activity;
    }

    function GenerateGuid() {
        function s4() {
            return Math.floor((1 + Math.random()) * 0x10000)
              .toString(16)
              .substring(1);
        }
        return s4() + s4() + '-' + s4() + '-' + s4() + '-' + s4() + '-' + s4() + s4() + s4();
    }

    function EnableMicButton() {
        jQuery(chatMicrophoneButton).prop('disabled', false);
        jQuery(chatMicrophoneButton).removeClass('disabled');
    }

    function DisableMicButton() {
        jQuery(chatMicrophoneButton).prop('disabled', true);
        jQuery(chatMicrophoneButton).addClass('disabled');
    }
});
