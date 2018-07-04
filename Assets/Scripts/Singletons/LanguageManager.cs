using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;
using System.Xml;

public class LanguageManager : MonoBehaviour {
    public enum ELanguage {
        en,
        zh,
        zht
    }

    public struct Dialogue {
        public List<Line> Lines;
        public string Key;
        public string ChoiceKey;

        public Dialogue(List<Line> lines, string key, string choiceKey) {
            Lines = lines;
            Key = key;
            ChoiceKey = choiceKey;
        }
    }

    public struct Line {
        public string Character;
        public string Sentence;

        public Line(string character, string sentence) {
            Character = character;
            Sentence = sentence;
        }

        public override string ToString() {
            return Character + ": \"" + Sentence + "\"";
        }
    }

    public struct Choice {
        public string ResponseKey;
        public string Content;

        public Choice(string responseKey, string content) {
            ResponseKey = responseKey;
            Content = content;
        }

        public override string ToString() {
            return ResponseKey + " / " + Content;
        }
    }

    public static LanguageManager Instance;
    public ELanguage Language = ELanguage.en;

    public TextAsset[] LanguageXmls;
    public TextAsset BannedWords;
    
    public static List<string> AnimalNames = new List<string> {
        "Fox", "Owl", "Wolf", "Bear", "Stag", "Doe", "Fawn", "Boar", "Moose", "Rabbit", "Flower"
    };
    
    private static List<string> characterNames = new List<string> {
        "Charlene", "Elvis", "Beulah", "Peter", "Isaac", "Carol"
    };
    
    private static List<string> alternateNames = new List<string> {
        "Charlotte", "Elmer", "Beatrice", "Paul", "Izzy", "Caren"
    };
    
    private XmlDocument doc;
    private XmlNode root;

    private XmlNode itemsNode;
    private XmlNode dialoguesNode;
    private XmlNode choicesNode;
    private XmlNode hintNode;
    private XmlNode uiNode;

    private string[] bannedWords;
    public static string PlayerName;
    
    private static string overridenCharacterName;
    private static string alternateName = string.Empty;
    
    void Start() {
        bannedWords = BannedWords.text.Split('\n');
    }
    
    public bool IsLegal(string proposedName) {
        if (proposedName.Length == 0 || proposedName.Trim().Length == 0) {
            GameManager.Instance.LogEvent("Name Typed", "empty_string", "Illegal", 1);
            return false;
        }
        
        if (bannedWords.Contains(proposedName.ToLower())) {
            GameManager.Instance.LogEvent("Name Typed", proposedName, "Illegal", 1);
            return false;
        }
        
        if (AnimalNames.Contains(proposedName)) {
            GameManager.Instance.LogEvent("Name Typed", "Animal name - " + proposedName, "Illegal", 1);
            return false;
        }

        foreach (string bannedWord in bannedWords) {
            if (proposedName.ToLower().Contains(bannedWord)) {
                GameManager.Instance.LogEvent("Name Typed", proposedName, "Illegal", 1);
                return false;
            }
        }

        for (int i = 0; i < characterNames.Count; i++) {
            string characterName = characterNames[i];    
            if (string.Compare(proposedName, characterName, true, CultureInfo.InvariantCulture) == 0) {
                overridenCharacterName = characterName;
                alternateName = alternateNames[i];
                break;
            }
        }

        PlayerName = proposedName;
        GameManager.Instance.LogEvent("Name Typed", proposedName, "Legal", 1);
        // legal 1
        return true;
    }
    private void Awake() {
        if (Instance == null) {
            Instance = this;
        }

        LoadXmlDoc();
    }

    private void LoadXmlDoc() {
        doc = new XmlDocument();
        doc.LoadXml(LanguageXmls[(int) Language].text);
        //doc.Load(Application.dataPath + "/Resources/" + Language.ToString() + ".xml");
        root = doc.SelectSingleNode("translation");

        // reset loaded node lists
        itemsNode = null;
        dialoguesNode = null;
    }

    private void LoadDocIfNotAlready() {
        if (doc == null || root == null) {
            LoadXmlDoc();
        }
    }

    public void SetLanguage(ELanguage eLanguage) {
        Language = eLanguage;
        LoadXmlDoc();
    }

    public string GetItem(string key) {
        LoadDocIfNotAlready();

        if (itemsNode == null) {
            itemsNode = root.SelectSingleNode("items");
        }

        XmlNode node = itemsNode.SelectSingleNode(key);
        return node != null ? ProcessString(node.InnerText) : "NULL";
    }

    public string GetHint(string key) {
        LoadDocIfNotAlready();
        if (hintNode == null) {
            hintNode = root.SelectSingleNode("hints");
        }

        XmlNode node = hintNode.SelectSingleNode(key);
        return node != null ? ProcessString(node.InnerText) : "NULL";
    }
    
    public string GetUI(string key) {
        LoadDocIfNotAlready();
        if (uiNode == null) {
            uiNode = root.SelectSingleNode("ui");
        }

        XmlNode node = uiNode.SelectSingleNode(key);
        return node != null ? ProcessString(node.InnerText) : "NULL";
    }

    public Dialogue GetDialogue(string dialogueName) {
        LoadDocIfNotAlready();

        if (dialoguesNode == null) {
            dialoguesNode = root.SelectSingleNode("dialogues");
        }

        List<Line> lines = new List<Line>();

        XmlNode dialogueNode = dialoguesNode.SelectSingleNode(dialogueName);
        XmlNodeList lineNodes = dialogueNode.SelectNodes("line");

        foreach (XmlNode node in lineNodes) {
            Line line = new Line(ProcessString(ProcessNameString(node.Attributes["character"].Value)), ProcessString(node.InnerText));
            lines.Add(line);
        }

        if (lines.Count == 0) {
            lines.Add(new Line("Error", "No lines were returned"));
            Debug.LogError("Language get dialogue error");
        }

        string choiceKey = dialogueNode.Attributes["choiceKey"] == null ? "NULL" : dialogueNode.Attributes["choiceKey"].Value;
        return new Dialogue(lines, dialogueName, choiceKey);
    }
    
    public List<Choice> GetChoices(string choiceKey) {
        LoadDocIfNotAlready();

        if (choicesNode == null) {
            choicesNode = root.SelectSingleNode("choices");
        }

        List<Choice> choices = new List<Choice>();

        XmlNode choiceNode = choicesNode.SelectSingleNode(choiceKey);
        XmlNodeList nodes = choiceNode.SelectNodes("choice");

        foreach (XmlNode node in nodes) {
            Choice choice = new Choice(node.Attributes["responseKey"].Value, ProcessString(node.InnerText));
            choices.Add(choice);
        }

        return choices;
    }

    private static string ProcessNameString(string str) {
        string newStr = str.Substring(0, 1).ToUpper() + str.Substring(1);
        foreach (string name in AnimalNames) {
            if (newStr.Contains(name)) {
                newStr = newStr.Replace(name, "<sprite name=\"" + name.ToLower() + "_face\"> " + name);
            }
        }

        return newStr;
    }
    private static string ProcessString(string str) {
        if (alternateName != string.Empty) {
            str = str.Replace(overridenCharacterName, alternateName);
        }
        
        return str.
            Replace("|i|", "<i>").
            Replace("|/i|", "</i>").
            Replace("|b|", "<b>").
            Replace("|/b|", "</b>").
            Replace("|c#", "<color=#").
            Replace("#c|", ">").
            Replace("|/c|", "</color>").
            Replace("|icon#", "<size=300%><sprite name=\"").
            Replace("#icon|", "\"></size>").
            Replace("|size#", "<size=").
            Replace("#size|", "%>").
            Replace("|/size|", "</size>").
            Replace("PLAYER_NAME", PlayerName);
    }
}