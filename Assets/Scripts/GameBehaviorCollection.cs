using System.Collections.Generic;

[System.Serializable]
public class GameBehaviorCollection {

	List<GameBehavior> behaviors = new List<GameBehavior>();

	public bool IsEmpty => behaviors.Count == 0;

	public void Add (GameBehavior behavior) {
		behaviors.Add(behavior);
	}

	public void Clear () {
		for (int i = 0; i < behaviors.Count; i++) {
			behaviors[i].Recycle();
		}
		behaviors.Clear();
	}

	public void GameUpdate () {
		for (int i = 0; i < behaviors.Count; i++) {
            //trigger behavior gameUpdate()
			if (!behaviors[i].GameUpdate()) {
                //if fails, find invalid location
				int lastIndex = behaviors.Count - 1;
                //duplicate the last one here.
				behaviors[i] = behaviors[lastIndex];
                //remove the last one.
				behaviors.RemoveAt(lastIndex);
                //redo
				i -= 1;
			}
		}
	}
}