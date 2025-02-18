﻿Imports System.Threading
Public Class Form4
    Private Declare Function GetTickCount64 Lib "kernel32" () As Long
    Private Declare Function GetAsyncKeyState Lib "user32" (ByVal vkey As Integer) As Short

    Public Declare Function mciSendString Lib "winmm.dll" Alias "mciSendStringA" _
    (ByVal lpstrCommand As String, ByVal lpstrReturnString As String, ByVal uReturnLength As _
     Integer, ByVal hwndCallback As Integer) As Integer
    Dim brush1 As SolidBrush
    Dim brush2 As SolidBrush
    Dim timeGauge As Integer

    Dim isStart As Boolean = False
    Dim isEnd As Boolean = False

    Dim startTime As Long
    Dim currentTime As Long
    Dim lastTime As Long
    Dim lastPlayerAnimTime As Long

    Dim backImage As Image
    Dim backBitmap As Bitmap

    Dim ldImage As Image
    Dim ldBitmap As Bitmap

    Dim playerImage As Image
    Dim playerBitmap(5) As Bitmap

    Dim monsterImage As Image
    Dim monsterBitmap As Bitmap

    Dim thread_main As Thread
    Structure Rect
        Dim x As Integer
        Dim y As Integer
        Dim height As Integer
        Dim width As Integer
    End Structure

    Dim backMap As Rect
    Dim backMap2 As Rect

    Dim ldRect As Rect
    Dim ldRect2 As Rect
    Dim ldRect3 As Rect
    Structure CharInfo
        Dim hp As Integer
        Dim pos As Rect
        Dim anim As Integer
        Dim state As Integer '0 : idle, 1 : move, 2 : attack, 3 : hit, 4 : die
    End Structure

    Dim plrInfo As CharInfo
    Dim isJump As Boolean
    Dim isJumpEffectSound As Boolean

    Dim velocity As Integer = 28

    Private Sub Form4_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        thread_main = New Thread(AddressOf Main) With {.IsBackground = True}

        My.Computer.Audio.Play("sound\hanggu.wav", AudioPlayMode.BackgroundLoop)
        mciSendString("open sound\jump.wav alias jumpsound1", 0, 0, 0)
    End Sub
    Private Sub Form4_Shown(sender As Object, e As EventArgs) Handles MyBase.Shown
        thread_main.Start()
        LoadBitmap()
        SpawnPlayer()

        backMap.x = 0
        backMap.width = 1498
        backMap.height = 662
        backMap2.x = 1498
        backMap2.width = 1498
        backMap2.height = 662

        ldRect.x = 0
        ldRect.y = 450 - 75
        ldRect.width = 665
        ldRect.height = 59
        ldRect2.x = 665
        ldRect2.y = 450 - 75
        ldRect2.width = 665
        ldRect2.height = 59
        ldRect3.x = 1330
        ldRect3.y = 450 - 75
        ldRect3.width = 665
        ldRect3.height = 59

        brush1 = New SolidBrush(Color.Black)
        brush2 = New SolidBrush(Color.Red)

        timeGauge = 0
        isStart = True
    End Sub
    Sub SpawnPlayer()
        plrInfo.hp = 3
        plrInfo.pos.x = 100
        plrInfo.pos.y = 287
        plrInfo.anim = 0
        plrInfo.state = 0

        isJump = False
        isJumpEffectSound = False
    End Sub
    Sub LoadBitmap()
        backImage = My.Resources.ResourceManager.GetObject("backImage")
        backBitmap = New Bitmap(backImage)
        backBitmap.MakeTransparent()

        ldImage = My.Resources.ResourceManager.GetObject("loadimage")
        ldBitmap = New Bitmap(ldImage)
        ldBitmap.MakeTransparent()

        monsterImage = My.Resources.ResourceManager.GetObject("junior_lace")
        monsterBitmap = New Bitmap(monsterImage)
        monsterBitmap.MakeTransparent()

        playerImage = My.Resources.ResourceManager.GetObject("Char_9_move")
        playerBitmap(0) = New Bitmap(playerImage)
        playerBitmap(0).MakeTransparent()

        playerImage = My.Resources.ResourceManager.GetObject("Char_10_move")
        playerBitmap(1) = New Bitmap(playerImage)
        playerBitmap(1).MakeTransparent()

        playerImage = My.Resources.ResourceManager.GetObject("Char_11_move")
        playerBitmap(2) = New Bitmap(playerImage)
        playerBitmap(2).MakeTransparent()

        playerImage = My.Resources.ResourceManager.GetObject("Char_12_move")
        playerBitmap(3) = New Bitmap(playerImage)
        playerBitmap(3).MakeTransparent()

        playerImage = My.Resources.ResourceManager.GetObject("Char_28_jump_move")
        playerBitmap(4) = New Bitmap(playerImage)
        playerBitmap(4).MakeTransparent()
    End Sub

    Private Sub Main()
        MsgBox("학업을 위해 옆 동네 도서관에 간 사이에" & vbCrLf & vbCrLf & "마을이 몬스터에 의해 공격받고 있다는 소식을 들었습니다." & vbCrLf & vbCrLf &
            "이대로 가다간 마을 주민들이 몬스터들에게 당해" & vbCrLf & vbCrLf & "코딩 노예가 될 것 입니다." & vbCrLf & vbCrLf & "가는 길에 마주치는 몬스터를 상대할 시간도 없습니다." & vbCrLf & vbCrLf &
            "몬스터들을 피해 빨리 마을로 달려가야겠습니다." & vbCrLf & vbCrLf & vbCrLf & "- Alt : 점프",, "스토리")

        startTime = GetTickCount64()
        currentTime = GetTickCount64()
        lastTime = GetTickCount64()
        lastPlayerAnimTime = GetTickCount64()

        Do
            currentTime = GetTickCount64()

            If currentTime > lastTime + 33 Then
                lastTime = currentTime
                ScrollMap()
                SetState()
                SwichPlayerAnim()

                MonsterSpawn()
                MonsterMove()

                Invoke(Sub() Me.Invalidate())

                Jump()

            End If

        Loop

    End Sub

    Sub ScrollMap()
        backMap.x -= 1
        backMap2.x -= 1

        If backMap.x <= -backMap.width Then
            backMap.x = backMap.width
        End If
        If backMap2.x <= -backMap2.width Then
            backMap2.x = backMap2.width
        End If

        ldRect.x -= 6
        ldRect2.x -= 6
        ldRect3.x -= 6

        If ldRect.x <= -ldRect.width Then
            ldRect.x = ldRect3.x + ldRect.width
        End If
        If ldRect2.x <= -ldRect2.width Then
            ldRect2.x = ldRect.x + ldRect2.width
        End If
        If ldRect3.x <= -ldRect3.width Then
            ldRect3.x = ldRect2.x + ldRect3.width
        End If
    End Sub
    Sub SetState()
        If GetAsyncKeyState(Keys.Menu) And isJump = False Then    'alt key input
            isJumpEffectSound = True
            plrInfo.state = 1
            isJump = True
        End If
    End Sub
    Sub SwichPlayerAnim()
        If plrInfo.state = 0 Then
            If plrInfo.anim >= 0 And plrInfo.anim <= 3 Then     'idle anim ++
                If currentTime > lastPlayerAnimTime + 100 Then     'idle anim swiching cooltime
                    lastPlayerAnimTime = currentTime

                    plrInfo.anim += 1
                    If plrInfo.anim > 3 Then    'left idle init
                        plrInfo.anim = 0
                    End If
                End If
            Else                                'left idle init
                plrInfo.anim = 0
            End If
        ElseIf plrInfo.state = 1 Then       'jump
            plrInfo.anim = 4
        End If
    End Sub
    Sub Jump()
        If isJump = True Then
            plrInfo.pos.y -= velocity
            velocity -= 4

            If plrInfo.pos.y >= 287 Then
                velocity = 28
                isJump = False
                plrInfo.state = 0
            End If
        End If

    End Sub

    Sub MonsterSpawn()

    End Sub

    Sub MonsterMove()

    End Sub

    Private Sub Form4_Paint(sender As Object, e As PaintEventArgs) Handles MyBase.Paint
        Label1.Text = CInt((currentTime - startTime) / 1000) & "초"

        e.Graphics.DrawImage(backBitmap, backMap.x, 0, backBitmap.Width, backBitmap.Height)
        e.Graphics.DrawImage(backBitmap, backMap2.x, 0, backBitmap.Width, backBitmap.Height)

        e.Graphics.DrawImage(ldBitmap, ldRect.x, ldRect.y, ldBitmap.Width, ldBitmap.Height)
        e.Graphics.DrawImage(ldBitmap, ldRect2.x, ldRect2.y, ldBitmap.Width, ldBitmap.Height)
        e.Graphics.DrawImage(ldBitmap, ldRect3.x, ldRect3.y, ldBitmap.Width, ldBitmap.Height)

        If timeGauge >= Me.Width - 70 Then
            plrInfo.pos.x += 3
        End If

        If plrInfo.pos.x < Me.Width - 10 Then
            e.Graphics.DrawImage(playerBitmap(plrInfo.anim), plrInfo.pos.x, plrInfo.pos.y, playerBitmap(plrInfo.anim).Width, playerBitmap(plrInfo.anim).Height)
        ElseIf isEnd = False Then
            isEnd = True
            MsgBox("열심히 달려 제시간에 마을에 도착했습니다." & vbCrLf & vbCrLf & "확인 버튼을 누르면 다음 스테이지로 이동합니다",, "NEXT STAGE")
            thread_main.Abort()
            Form1.Show()
            Me.Close()
        End If
        If isStart = True Then
            e.Graphics.FillRectangle(brush1, 20, 20, 10, 20)
            e.Graphics.FillRectangle(brush2, 30, 20, timeGauge, 20)

            e.Graphics.FillRectangle(brush1, Me.Width - 40, 20, 10, 20)

            If timeGauge < Me.Width - 70 Then
                timeGauge = CInt((Me.Width - 70) / 5) * CInt((currentTime - startTime) / 1000)  '600 / 200으로 바꾸자
            End If

            If timeGauge >= Me.Width - 70 Then
                timeGauge = Me.Width - 70
            End If
        End If

            If isJumpEffectSound = True And isJump = True Then
            isJumpEffectSound = False
            mciSendString("play jumpsound1 from 0", 0, 0, 0)
        End If
    End Sub
    Private Sub Form4_Closed(sender As Object, e As EventArgs) Handles MyBase.Closed
        thread_main.Abort()
    End Sub

    Private Sub Button1_MouseDown(sender As Object, e As MouseEventArgs) Handles Button1.MouseDown
        Button1.BackgroundImage = My.Resources.ResourceManager.GetObject("button21")
    End Sub

    Private Sub Button1_MouseUp(sender As Object, e As MouseEventArgs) Handles Button1.MouseUp
        thread_main.Abort()
        Form2.Show()
        Me.Close()
    End Sub
End Class