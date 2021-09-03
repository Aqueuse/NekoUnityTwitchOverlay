using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class Neko : MonoBehaviour{
    public GameObject followerObject;
    public Transform followerTransform;
    public Rigidbody2D nekoRigidbody;
    public Animator nekoAnimator;

    private Transform _nekoTransform;
    private Rigidbody2D _followerRigidbody2D;
    private TextMesh _followerTextMesh;

    private bool _catched;
    private bool _hasChanged;
    private bool _isIdle;

    private float _nekoPositionX;
    private float _nekoPositionY;

    private float _followerPositionX;
    private float _followerPositionY;

    private float _deltaX;
    private float _deltaY;
    private const float Speed = 3.5f;

    private string _follower;
    
    private static readonly int Idle = Animator.StringToHash("idle");
    private static readonly int MoveBottom = Animator.StringToHash("moveBottom");
    private static readonly int MoveTop = Animator.StringToHash("moveTop");
    private static readonly int MoveLeft = Animator.StringToHash("moveLeft");
    private static readonly int MoveRight = Animator.StringToHash("moveRight");
    private static readonly int MoveTopLeft = Animator.StringToHash("moveTopLeft");
    private static readonly int MoveTopRight = Animator.StringToHash("moveTopRight");
    private static readonly int MoveBottomLeft = Animator.StringToHash("moveBottomLeft");
    private static readonly int MoveBottomRight = Animator.StringToHash("moveBottomRight");



    private void Start() {
        Application.runInBackground = true;
        _followerTextMesh = followerObject.GetComponent<TextMesh>();
        _followerRigidbody2D = followerObject.GetComponent<Rigidbody2D>();
        _nekoTransform = gameObject.GetComponent<Transform>();

        _followerTextMesh.text = GetFollowerName();

        var fileSystemWatcher = new FileSystemWatcher();
        fileSystemWatcher.Path = @".";
        fileSystemWatcher.Changed += OnFileChange;
        fileSystemWatcher.EnableRaisingEvents = true;
    }

    private void Update() {
        var step = Speed * Time.deltaTime;
        if (_hasChanged) {
            _catched = false;
            _isIdle = false;
            _hasChanged = false;
            _followerTextMesh.text = _follower;
            followerObject.transform.position = new Vector2(Random.value * 5, Random.value * 5);
            _followerRigidbody2D.AddForce(new Vector2(Random.value * 5, Random.value * 5), ForceMode2D.Impulse);
        }

        switch (_catched) {
            case true:
                if (!_isIdle) {
                    nekoRigidbody.velocity = Vector2.zero;
                    _followerRigidbody2D.velocity = Vector2.zero;
                    _followerTextMesh.text = "";
                    nekoAnimator.SetTrigger(Idle);
                    _isIdle = true;
                }
                
                break;
            case false:
                var nekoPosition = _nekoTransform.position;
                var followerPosition = followerTransform.position;

                transform.position = Vector2.MoveTowards(nekoPosition, followerPosition, step);

                _nekoPositionX = nekoPosition.x;
                _nekoPositionY = nekoPosition.y;

                _followerPositionY = followerPosition.y;
                _followerPositionX = followerPosition.x;

                _deltaX = _nekoPositionX - _followerPositionX;
                _deltaY = _nekoPositionY - _followerPositionY;
                
                if (Math.Abs((int) _deltaX) < 1) {
                    if (_deltaY > 0) {
                        nekoAnimator.SetTrigger(MoveBottom);
                        return;
                    }

                    if (_deltaY < 0) {
                        nekoAnimator.SetTrigger(MoveTop);
                        return;
                    }
                }

                if (Math.Abs((int) _deltaY) < 1) {
                    print("horizontal move");
                    if (_deltaX < 0) {
                        print("right");
                        nekoAnimator.SetTrigger(MoveRight);
                        return;
                    }

                    if (_deltaX > 0) {
                        nekoAnimator.SetTrigger(MoveLeft);
                        return;
                    }
                }

                // Diagonales

                if (_deltaX > 0 && _deltaY < 0) {
                    nekoAnimator.SetTrigger(MoveTopLeft);
                }

                if (_deltaX < 0 && _deltaY < 0) {
                    nekoAnimator.SetTrigger(MoveTopRight);
                }

                if (_deltaX > 0 && _deltaY > 0) {
                    nekoAnimator.SetTrigger(MoveBottomLeft);
                }

                if (_deltaX < 0 && _deltaY > 0) {
                    nekoAnimator.SetTrigger(MoveBottomRight);
                }

                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name != "follower_text") return;
        _catched = true;
    }

    private string GetFollowerName() {
        var savedJson = File.ReadAllText(@"follow.json");
        var rawJson = (JObject) JsonConvert.DeserializeObject(savedJson);
        if (rawJson != null) _follower = (string) rawJson["username"];
        return _follower;
    }

    private void OnFileChange(object source, FileSystemEventArgs fileSystemEventArgs) {
        if (!fileSystemEventArgs.Name.EndsWith("follow.json")) return;
        _hasChanged = true;
        _follower = GetFollowerName();
    }
}