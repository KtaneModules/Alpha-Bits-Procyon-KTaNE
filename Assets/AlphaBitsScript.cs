using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using KModkit;

public class AlphaBitsScript : MonoBehaviour {

		public KMBombInfo bomb;
		public KMAudio audio;

		public KMSelectable arrowUp;
		public KMSelectable arrowDown;
		public KMSelectable buttonSubmit;

		private int[] operationIndices = new int[2];
		private string[] alphabet = new string[32] {"0", "1", "2", "3", "4", "5", "6", "7", "8", "9", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V"};
		private int alphabetIndex;
		private string[,] letters = new string[2,2]; // stage, letter
		private string[] operations = new string[8] {"OR", "AND", "XOR", "IMP", "NOR", "NAND", "XNOR", "IMPBY"};

		public TextMesh displayTL;
		public TextMesh displayML;
		public TextMesh displayBL;
		public TextMesh displayTR;
		public TextMesh displayMR;
		public TextMesh displayBR;
		public TextMesh bigDisplay;
		public TextMesh idNumber;

		public AudioClip[] sounds;

		private int stage = 1;
		private int answerIndex = 0;
		static int moduleIdCounter = 1;
		int moduleId;
		private bool moduleSolved;

		void Awake() {
				moduleId = moduleIdCounter++;

				arrowUp.OnInteract += delegate() { onArrowUpPress(arrowUp); return false; };
				arrowDown.OnInteract += delegate() { onArrowDownPress(arrowDown); return false; };
				buttonSubmit.OnInteract += delegate() { onSubmitPress(arrowDown); return false; };

				stage = 1;
		}

		void Start() {
				for(int i = 0; i < 2; i++) {
						operationIndices[i] = UnityEngine.Random.Range(0, 28);
				}

				alphabetIndex = UnityEngine.Random.Range(0,32);
				bigDisplay.text = " " + alphabet[alphabetIndex];
				idNumber.text = "ID: ";

				PickAnswers();
				SetValues1(letters[0,0], letters[0,1]);
				SetValues2(letters[1,0], letters[1,1]);

				Debug.LogFormat("[Alpha-Bits #{0}] Stage 1 answers are " + letters[0,0] + " (" + ToBin(Array.IndexOf(alphabet, letters[0,0]), 5) + ")" + " and " + letters[0,1] + " (" + ToBin(Array.IndexOf(alphabet, letters[0,1]), 5) + ")", moduleId);
				Debug.LogFormat("[Alpha-Bits #{0}] Stage 2 answers are " + letters[1,0] + " (" + ToBin(Array.IndexOf(alphabet, letters[1,0]), 5) + ")" + " and " + letters[1,1] + " (" + ToBin(Array.IndexOf(alphabet, letters[1,1]), 5) + ")", moduleId);
		}

		void PickAnswers() {
				int[,] numbers = new int[2,2];
				for(int i = 0; i < 2; i++) {
						for(int j = 0; j < 2; j++) {
								numbers[i,j] = -1;
						}
				}

				numbers[0,0] = UnityEngine.Random.Range(0, 32);
				do {
						numbers[0,1] = UnityEngine.Random.Range(0, 32);
				} while(numbers[0,1] == numbers[0,0]);
				do {
						numbers[1,0] = UnityEngine.Random.Range(0, 32);
				} while(numbers[1,0] == numbers[0,1] || numbers[1,0] == numbers[0,0]);
				do {
						numbers[1,1] = UnityEngine.Random.Range(0, 32);
				} while(numbers[1,1] == numbers[1,0] || numbers[1,1] == numbers[0,1] || numbers[1,1] == numbers[0,0]);

				for(int i = 0; i < 2; i++) {
						for(int j = 0; j < 2; j++) {
								letters[i,j] = alphabet[numbers[i,j]];
						}
				}
		}

		void SetValues1(string s1, string s2) { // 0=OR, 1=AND, 2=XOR, 3=IMP, 4=NOR, 5=NAND, 6=XNOR, 7=IMPBY
				int a = -1;
				int b = -1;
				char l1 = s1[0];
				char l2 = s2[0];
				if(l1 > 64) {
						a = l1 - 55;
				} else if(l1 > 47) {
						a = l1 - 48;
				}
				if(l2 > 64) {
						b = l2 - 55;
				} else if(l2 > 47) {
						b = l2 - 48;
				}

				SortedList<string, int[]> operationsList = new SortedList<string, int[]>() {
						{ "013", new int[3] {a | b, 					 a & b, 					(31-a) | b} },
						{ "017", new int[3] {a | b, 					 a & b, 					a | (31-b)} },
						{ "023", new int[3] {a | b, 					 a ^ b, 					(31-a) | b} },
						{ "027", new int[3] {a | b, 					 a ^ b, 					a | (31-b)} },
						{ "053", new int[3] {a | b, 					 (31-a) | (31-b), (31-a) | b} },
						{ "057", new int[3] {a | b, 					 (31-a) | (31-b), a | (31-b)} },
						{ "063", new int[3] {a | b, 					 (31-a) ^ b,			(31-a) | b} },
						{ "067", new int[3] {a | b, 					 (31-a) ^ b,			a | (31-b)} },
						{ "037", new int[3] {a | b, 					 (31-a) | b,			a | (31-b)} },
						{ "123", new int[3] {a & b, 					 a ^ b, 					(31-a) | b} },
						{ "127", new int[3] {a & b, 					 a ^ b, 					a | (31-b)} },
						{ "143", new int[3] {a & b, 					 (31-a) & (31-b), (31-a) | b} },
						{ "147", new int[3] {a & b, 					 (31-a) & (31-b), a | (31-b)} },
						{ "163", new int[3] {a & b, 					 (31-a) ^ b,			(31-a) | b} },
						{ "167", new int[3] {a & b, 					 (31-a) ^ b,			a | (31-b)} },
						{ "137", new int[3] {a & b, 					 (31-a) | b,			a | (31-b)} },
						{ "243", new int[3] {a ^ b, 					 (31-a) & (31-b), (31-a) | b} },
						{ "247", new int[3] {a ^ b, 					 (31-a) & (31-b), a | (31-b)} },
						{ "253", new int[3] {a ^ b, 					 (31-a) | (31-b), (31-a) | b} },
						{ "257", new int[3] {a ^ b, 					 (31-a) | (31-b), a | (31-b)} },
						{ "453", new int[3] {(31-a) & (31-b), (31-a) | (31-b), (31-a) | b} },
						{ "457", new int[3] {(31-a) & (31-b), (31-a) | (31-b), a | (31-b)} },
						{ "463", new int[3] {(31-a) & (31-b), (31-a) ^ b,			(31-a) | b} },
						{ "467", new int[3] {(31-a) & (31-b), (31-a) ^ b,			a | (31-b)} },
						{ "437", new int[3] {(31-a) & (31-b), (31-a) | b,			a | (31-b)} },
						{ "563", new int[3] {(31-a) | (31-b), (31-a) ^ b,			(31-a) | b} },
						{ "567", new int[3] {(31-a) | (31-b), (31-a) ^ b,			a | (31-b)} },
						{ "537", new int[3] {(31-a) | (31-b), (31-a) | b,			a | (31-b)} },
				};

				int[,] ops = new int[2,3]; // column row
				for(int i = 0; i < 2; i++) {
						ops[i,0] = UnityEngine.Random.Range(0,3);
						do {
								ops[i,1] = UnityEngine.Random.Range(0,3);
						} while(ops[i,1] == ops[i,0]);
						ops[i,2] = 3 - ops[i,0] - ops[i,1];
				}

				string letterT = ConvertToText(operationsList.Values[operationIndices[0]][ops[0,0]]);
				string letterM = ConvertToText(operationsList.Values[operationIndices[0]][ops[0,1]]);
				string letterB = ConvertToText(operationsList.Values[operationIndices[0]][ops[0,2]]);

				displayTL.text = " " + letterT;
				idNumber.text += operationsList.Keys[operationIndices[0]][ops[0,0]];
				displayML.text = " " + letterM;
				idNumber.text += operationsList.Keys[operationIndices[0]][ops[0,1]];
				displayBL.text = " " + letterB;
				idNumber.text += operationsList.Keys[operationIndices[0]][ops[0,2]];

				Debug.LogFormat("[Alpha-Bits #{0}] Stage 1: Top display is" + displayTL.text + "; " +    "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[0]][ops[0,0]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterT), 5), moduleId);
				Debug.LogFormat("[Alpha-Bits #{0}] Stage 1: Middle display is" + displayML.text + "; " + "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[0]][ops[0,1]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterM), 5), moduleId);
				Debug.LogFormat("[Alpha-Bits #{0}] Stage 1: Bottom display is" + displayBL.text + "; " + "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[0]][ops[0,2]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterB), 5), moduleId);
		}

		void SetValues2(string s1, string s2) { // 0=OR, 1=AND, 2=XOR, 3=IMP, 4=NOR, 5=NAND, 6=XNOR, 7=IMPBY
				int a = -1;
				int b = -1;
				char l1 = s1[0];
				char l2 = s2[0];
				if(l1 > 64) {
						a = l1 - 55;
				} else if(l1 > 47) {
						a = l1 - 48;
				}
				if(l2 > 64) {
						b = l2 - 55;
				} else if(l2 > 47) {
						b = l2 - 48;
				}

				SortedList<string, int[]> operationsList = new SortedList<string, int[]>() {
						{ "013", new int[3] {a | b, 					 a & b, 					(31-a) | b} },
						{ "017", new int[3] {a | b, 					 a & b, 					a | (31-b)} },
						{ "023", new int[3] {a | b, 					 a ^ b, 					(31-a) | b} },
						{ "027", new int[3] {a | b, 					 a ^ b, 					a | (31-b)} },
						{ "053", new int[3] {a | b, 					 (31-a) | (31-b), (31-a) | b} },
						{ "057", new int[3] {a | b, 					 (31-a) | (31-b), a | (31-b)} },
						{ "063", new int[3] {a | b, 					 (31-a) ^ b,			(31-a) | b} },
						{ "067", new int[3] {a | b, 					 (31-a) ^ b,			a | (31-b)} },
						{ "037", new int[3] {a | b, 					 (31-a) | b,			a | (31-b)} },
						{ "123", new int[3] {a & b, 					 a ^ b, 					(31-a) | b} },
						{ "127", new int[3] {a & b, 					 a ^ b, 					a | (31-b)} },
						{ "143", new int[3] {a & b, 					 (31-a) & (31-b), (31-a) | b} },
						{ "147", new int[3] {a & b, 					 (31-a) & (31-b), a | (31-b)} },
						{ "163", new int[3] {a & b, 					 (31-a) ^ b,			(31-a) | b} },
						{ "167", new int[3] {a & b, 					 (31-a) ^ b,			a | (31-b)} },
						{ "137", new int[3] {a & b, 					 (31-a) | b,			a | (31-b)} },
						{ "243", new int[3] {a ^ b, 					 (31-a) & (31-b), (31-a) | b} },
						{ "247", new int[3] {a ^ b, 					 (31-a) & (31-b), a | (31-b)} },
						{ "253", new int[3] {a ^ b, 					 (31-a) | (31-b), (31-a) | b} },
						{ "257", new int[3] {a ^ b, 					 (31-a) | (31-b), a | (31-b)} },
						{ "453", new int[3] {(31-a) & (31-b), (31-a) | (31-b), (31-a) | b} },
						{ "457", new int[3] {(31-a) & (31-b), (31-a) | (31-b), a | (31-b)} },
						{ "463", new int[3] {(31-a) & (31-b), (31-a) ^ b,			(31-a) | b} },
						{ "467", new int[3] {(31-a) & (31-b), (31-a) ^ b,			a | (31-b)} },
						{ "437", new int[3] {(31-a) & (31-b), (31-a) | b,			a | (31-b)} },
						{ "563", new int[3] {(31-a) | (31-b), (31-a) ^ b,			(31-a) | b} },
						{ "567", new int[3] {(31-a) | (31-b), (31-a) ^ b,			a | (31-b)} },
						{ "537", new int[3] {(31-a) | (31-b), (31-a) | b,			a | (31-b)} },
				};

				int[,] ops = new int[2,3]; // column row
				for(int i = 0; i < 2; i++) {
						ops[i,0] = UnityEngine.Random.Range(0,3);
						do {
								ops[i,1] = UnityEngine.Random.Range(0,3);
						} while(ops[i,1] == ops[i,0]);
						ops[i,2] = 3 - ops[i,0] - ops[i,1];
				}

				string letterT = ConvertToText(operationsList.Values[operationIndices[1]][ops[1,0]]);
				string letterM = ConvertToText(operationsList.Values[operationIndices[1]][ops[1,1]]);
				string letterB = ConvertToText(operationsList.Values[operationIndices[1]][ops[1,2]]);

				displayTR.text = " " + letterT;
				idNumber.text += operationsList.Keys[operationIndices[1]][ops[1,0]];
				displayMR.text = " " + letterM;
				idNumber.text += operationsList.Keys[operationIndices[1]][ops[1,1]];
				displayBR.text = " " + letterB;
				idNumber.text += operationsList.Keys[operationIndices[1]][ops[1,2]];

				Debug.LogFormat("[Alpha-Bits #{0}] Stage 2: Top display is" + displayTR.text + "; " +    "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[1]][ops[1,0]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterT), 5), moduleId);
				Debug.LogFormat("[Alpha-Bits #{0}] Stage 2: Middle display is" + displayMR.text + "; " + "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[1]][ops[1,1]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterM), 5), moduleId);
				Debug.LogFormat("[Alpha-Bits #{0}] Stage 2: Bottom display is" + displayBR.text + "; " + "Answer1 " + operations[Convert.ToInt32(operationsList.Keys[operationIndices[1]][ops[1,2]] - 48)] + " Answer2 = " + ToBin(Array.IndexOf(alphabet, letterB), 5), moduleId);
		}

		string ConvertCharacterBin(char c) {
				if(c > 64) {
						return ToBin(c - 55, 5);
				} else if(c > 47) {
						return ToBin(c - 48, 5);
				}
				return "oops";
		}

		string ConvertToText(int i) {
				if(i < 10) {
						return i.ToString();
				} else {
						char c = (char)(i + 55);
						return c.ToString();
				}
		}

		public static string ToBin(int value, int len) {
				return (len > 1 ? ToBin(value >> 1, len - 1) : null) + "01"[value & 1];
		}

		void onArrowUpPress(KMSelectable arrow) {
				arrow.AddInteractionPunch(0.1f);
				alphabetIndex = (alphabetIndex + 1) % 32;
				bigDisplay.text = " " + alphabet[alphabetIndex];
		}

		void onArrowDownPress(KMSelectable arrow) {
				arrow.AddInteractionPunch(0.1f);
				alphabetIndex = (alphabetIndex + 31) % 32;
				bigDisplay.text = " " + alphabet[alphabetIndex];
		}

		void onSubmitPress(KMSelectable button) {
				button.AddInteractionPunch();
				if(stage == 1) {
						if(string.Equals(bigDisplay.text[1].ToString(), letters[0,answerIndex])) {
								Debug.LogFormat("[Alpha-Bits #{0}] Stage 1: Entered " + letters[0,answerIndex] + ", this is correct", moduleId);
								audio.PlaySoundAtTransform(sounds[0].name, transform);
								button.AddInteractionPunch(0.5f);
								answerIndex++;
								if(answerIndex == 2) {
										Debug.LogFormat("[Alpha-Bits #{0}] Stage 1 passed", moduleId);
										audio.PlaySoundAtTransform(sounds[2].name, transform);
										button.AddInteractionPunch(1f);
										answerIndex = 0;
										stage++;
										displayTL.text = "";
										displayML.text = "";
										displayBL.text = "";
								}
						} else {
								Debug.LogFormat("[Alpha-Bits #{0}] Stage 1: Entered" + bigDisplay.text[1] + ", this is incorrect - Strike given and stage reset", moduleId);
								button.AddInteractionPunch(1f);
								GetComponent<KMBombModule>().HandleStrike();
								answerIndex = 0;
						}
				} else if(stage == 2) {
					if(string.Equals(bigDisplay.text[1].ToString(), letters[1,answerIndex])) {
							Debug.LogFormat("[Alpha-Bits #{0}] Stage 2: Entered " + letters[1,answerIndex] + ", this is correct", moduleId);
							audio.PlaySoundAtTransform(sounds[0].name, transform);
							button.AddInteractionPunch(0.5f);
							answerIndex++;
							if(answerIndex == 2) {
									Debug.LogFormat("[Alpha-Bits #{0}] Stage 2 passed", moduleId);
									Debug.LogFormat("[Alpha-Bits #{0}] Module solved", moduleId);
									audio.PlaySoundAtTransform(sounds[1].name, transform);
									button.AddInteractionPunch(1f);
									idNumber.text = "ID: ~~~~~~";
									displayTL.text = " S";
									displayML.text = " O";
									displayBL.text = " L";
									bigDisplay.text = " ~";
									displayTR.text = " V";
									displayMR.text = " E";
									displayBR.text = " D";

									GetComponent<KMBombModule>().HandlePass();
							}
					} else {
								Debug.LogFormat("[Alpha-Bits #{0}] Stage 2: Entered" + bigDisplay.text[1] + ", this is incorrect - Strike given and stage reset", moduleId);
								GetComponent<KMBombModule>().HandleStrike();
								answerIndex = 0;
						}
				}
		}

		// TwitchPlays Code
		#pragma warning disable 414
    		private string TwitchHelpMessage = "Type '!{0} submit <letter1> <letter2>' to submit a full stage. Type '!{0} submit <letter>' to submit a single answer. The first stage must be submitted before the second.";
		#pragma warning restore 414

		protected IEnumerator ProcessTwitchCommand(string input) {
				var split = input.ToUpperInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

				if(split.Length < 2 || split.Length > 3) {
						yield return "sendtochat Invalid input, input cancelled";
						yield break;
				}

				if(split[0] != "SUBMIT") {
						yield return "sendtochat Invalid input, input cancelled";
						yield break;
				}

				for(int i = 1; i < split.Length; i++) {
						if(split[i].Length > 1) {
								yield return "sendtochat Invalid input, input cancelled";
								yield break;
						}
				}

				if(!alphabet.Contains(split[1][0].ToString())) {
						yield return "sendtochat Invalid character, input cancelled";
						yield break;
				} else if(split.Length == 3 && !alphabet.Contains(split[2][0].ToString())) {
						yield return "sendtochat Invalid character, input cancelled";
						yield break;
				}

				if(split.Length == 2 && split[1].Length == 1) {
						char c = split[1][0];
						while(bigDisplay.text[1] < c) {
								arrowUp.OnInteract();
								yield return new WaitForSeconds(0.05f);
						}
						while(bigDisplay.text[1] > c) {
								arrowDown.OnInteract();
								yield return new WaitForSeconds(0.05f);
						}

						yield return new KMSelectable[] { buttonSubmit };
				}

				if(split.Length == 3 && split[1].Length == 1 && split[2].Length == 1 && answerIndex == 0) {
						char[] c = new char[2] { split[1][0], split[2][0] };
						for(int i = 0; i < 2; i++) {
								while(bigDisplay.text[1] < c[i]) {
										arrowUp.OnInteract();
										yield return new WaitForSeconds(0.05f);
								}
								while(bigDisplay.text[1] > c[i]) {
										arrowDown.OnInteract();
										yield return new WaitForSeconds(0.05f);
								}

								yield return new KMSelectable[] { buttonSubmit };
								yield return new WaitForSeconds(0.5f);
						}
				}

				yield return null;
		}
}
