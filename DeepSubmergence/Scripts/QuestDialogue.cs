using System;

namespace DeepSubmergence {
    [Serializable]
    public class QuestDialogueChunk {
        public bool autoProgress;
        public string[] requiredItems;
        public string[] dialogueOnFinish;
        public string[] controlTags;
        public string[] frames;
    }

    [Serializable]
    public class QuestDialogue {
        public string saveId;
        public string speakerName;
        public int progress;
        public QuestDialogueChunk[] chunks;
    }
}