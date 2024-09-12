using System;

namespace DeepSubmergence {
    [Serializable]
    public class QuestDialogueChunk {
        public string[] requiredItems;
        public string[] dialogueOnFinish;
        public string[] controlTags;
        public string[][] possibleFrames;
    }

    [Serializable]
    public class QuestDialogue {
        public string saveId;
        public int progress;
        public QuestDialogueChunk[] chunks;
    }
}