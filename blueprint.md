Application: Session Terminator Tool
1. Project Overview
Source: ResetSessionUsersGUI.ps1 PowerShell script
Target: C# Windows Forms Application (.NET 8)
Purpose: Convert PowerShell GUI script with session management functionality to native C# application

2. Core Requirements
2.1 Functional Requirements
Session Management: Query and terminate remote sessions

Countdown Timer: Configurable timer with notifications

User Notifications: Send messages to active sessions

Dynamic Session Detection: Real-time session monitoring

Settings Persistence: JSON-based configuration

Real-time Output Display: Console-like output window

Process Control: Start/Stop execution

2.2 Non-Functional Requirements
Performance: Native C# execution (no PowerShell overhead)

Reliability: Proper exception handling

User Experience: Responsive UI with progress indicators

Maintainability: Clean architecture, separation of concerns

3. Architecture Blueprint
3.1 Project Structure
text
ResetSessionUsersApp/
├── ResetSessionUsersApp.csproj
├── Program.cs
├── Models/
│   ├── Session.cs
│   ├── AppSettings.cs
│   └── ExecutionResult.cs
├── Services/
│   ├── SessionService.cs
│   ├── NotificationService.cs
│   ├── SettingsService.cs
│   └── PowerShellRunner.cs
├── UI/
│   ├── MainForm.cs
│   ├── MainForm.Designer.cs
│   ├── SettingsForm.cs
│   └── SettingsForm.Designer.cs
├── Utilities/
│   ├── NativeMethods.cs
│   ├── ConsoleEmulator.cs
│   └── TimerHelper.cs
└── Resources/
    └── Icons/
3.2 Technology Stack
.NET 8 Windows Forms

System.Text.Json for settings

System.Diagnostics for process management

System.Management for WMI queries (alternative to query user)

System.Threading.Tasks for async operations

4. Component Specifications
4.1 Data Models
csharp
// Session.cs
public class SessionInfo
{
    public string Username { get; set; }
    public string SessionName { get; set; }
    public int SessionId { get; set; }
    public string State { get; set; }
    public TimeSpan IdleTime { get; set; }
    public DateTime LogonTime { get; set; }
}

// AppSettings.cs
public class AppSettings
{
    public string Server { get; set; } = "trts-testotk";
    public int TimerSeconds { get; set; } = 900;
    public int NotificationInterval { get; set; } = 60;
    public string ConfigPath { get; set; }
}
4.2 Service Layer
csharp
// SessionService.cs
public interface ISessionService
{
    Task<List<SessionInfo>> GetActiveSessionsAsync(string server);
    Task<bool> SendNotificationAsync(int sessionId, string server, string message);
    Task<bool> TerminateSessionAsync(int sessionId, string server);
}

// PowerShellRunner.cs
public class PowerShellRunner
{
    public event EventHandler<string> OutputReceived;
    public event EventHandler<string> ErrorReceived;
    
    public async Task<int> ExecuteScriptAsync(string scriptPath, Dictionary<string, string> parameters);
    public void CancelExecution();
}
4.3 UI Components
4.3.1 MainForm Specifications
Controls Required:

Input Panel (GroupBox)

TextBox: Server (with label)

NumericUpDown: Timer (seconds)

NumericUpDown: Notification Interval (seconds)

Control Buttons

Button: Start (primary action)

Button: Stop (secondary, disabled initially)

Button: Default Settings

Button: Clear Output

Output Display

RichTextBox: Read-only, monospace font

Scrollbars: Both

Context Menu: Copy/Clear

Status Area

StatusStrip with labels

ProgressBar for countdown

Event Handlers:

btnStart_Click: Validate input, start async execution

btnStop_Click: Cancel execution

Form_Closing: Cleanup resources

4.3.2 SettingsForm Specifications
Controls:

TextBox: Default Server

NumericUpDown: Default Timer

NumericUpDown: Default Interval

TextBox: Config Path (with Browse button)

Buttons: Save, Reset, Cancel

5. Key Algorithms & Logic
5.1 Session Query Algorithm
csharp
// Alternative to PowerShell's 'query user' command
public List<SessionInfo> QueryUserSessions(string server)
{
    // Option 1: WMI Query
    // SELECT * FROM Win32_LogonSession WHERE LogonType=2 OR LogonType=10
    
    // Option 2: Process 'query' command output parsing
    // Process.Start("query", $"user /server:{server}")
    
    // Option 3: Terminal Services API (wtsapi32.dll)
    // WTSEnumerateSessions
}
5.2 Countdown with Notifications
csharp
public async Task ExecuteCountdownAsync(
    string server, 
    int totalSeconds, 
    int intervalSeconds,
    IProgress<string> progress,
    CancellationToken cancellationToken)
{
    int remaining = totalSeconds;
    
    while (remaining > 0 && !cancellationToken.IsCancellationRequested)
    {
        // Send notifications at intervals
        if (remaining == totalSeconds || 
            (remaining % intervalSeconds == 0 && remaining < totalSeconds))
        {
            await SendNotificationsAsync(server, remaining);
        }
        
        // Update UI
        progress.Report($"Осталось: {remaining} сек");
        
        await Task.Delay(1000, cancellationToken);
        remaining--;
    }
}
5.3 Settings Management
json
// settings.json structure
{
  "DefaultSettings": {
    "Server": "trts-testotk",
    "TimerSeconds": 900,
    "NotificationInterval": 60
  },
  "LastUsedSettings": {
    "Server": "test-server",
    "TimerSeconds": 600,
    "NotificationInterval": 30
  },
  "Application": {
    "Version": "1.0",
    "LastRun": "2024-01-15T10:30:00"
  }
}
6. UI/UX Design Guidelines
6.1 Layout Principles
Responsive: Minimum 800x600 resolution support

Consistent: Standard Windows Forms controls

Accessible: Proper tab order, keyboard shortcuts

Informative: Status indicators, tooltips

6.2 Color Scheme
csharp
// Use SystemColors for consistency
ColorTheme = {
    Primary: SystemColors.Control,
    Secondary: SystemColors.Window,
    Success: Color.FromArgb(76, 175, 80),
    Warning: Color.FromArgb(255, 193, 7),
    Error: Color.FromArgb(244, 67, 54),
    Text: SystemColors.ControlText
}
6.3 Localization Support
csharp
// Resource files for multilingual support
Resources/
├── Strings.resx (default)
├── Strings.ru-RU.resx
└── Strings.en-US.resx
7. Implementation Phases
Phase 1: Foundation (Week 1)
Create Windows Forms project

Implement basic UI layout

Create data models

Implement settings service

Phase 2: Core Logic (Week 2)
Implement SessionService

Create PowerShell integration

Implement countdown logic

Add async/await support

Phase 3: UI Polish (Week 3)
Add progress indicators

Implement output console

Add validation

Create SettingsForm

Phase 4: Testing & Polish (Week 4)
Unit tests

Integration testing

Error handling

Performance optimization

8. Error Handling Strategy
8.1 Exception Categories
csharp
public enum ErrorCategory
{
    NetworkError,
    PermissionError,
    SessionError,
    ConfigurationError,
    ExecutionError
}
8.2 Recovery Strategies
Network issues: Retry with exponential backoff

Permission errors: Show user-friendly message

Configuration errors: Reset to defaults

Execution errors: Log and continue if possible

9. Security Considerations
9.1 Input Validation
csharp
public static class InputValidator
{
    public static bool ValidateServerName(string server)
    {
        // Check for valid server name
        return !string.IsNullOrWhiteSpace(server) &&
               server.Length <= 255 &&
               !server.Contains("..") &&
               !server.Contains("/") &&
               !server.Contains("\\");
    }
    
    public static bool ValidateTimeRange(int seconds)
    {
        return seconds > 0 && seconds <= 86400; // Max 24 hours
    }
}
9.2 Permission Requirements
Required: Local administrator rights

Network: RPC/WMI access to target server

Session: Terminal Services access

10. Build & Deployment
10.1 Build Configuration
xml
<!-- .csproj optimizations -->
<PropertyGroup>
  <OutputType>WinExe</OutputType>
  <TargetFramework>net8.0-windows</TargetFramework>
  <UseWindowsForms>true</UseWindowsForms>
  <ApplicationIcon>Resources\app.ico</ApplicationIcon>
  <AssemblyVersion>1.0.0.0</AssemblyVersion>
  <FileVersion>1.0.0.0</FileVersion>
  <Nullable>enable</Nullable>
  <PublishSingleFile>true</PublishSingleFile>
  <SelfContained>false</SelfContained>
</PropertyGroup>
10.2 Installation Options
Standalone EXE: Single file deployment

MSI Installer: For enterprise deployment

ClickOnce: Automatic updates

11. Testing Strategy
11.1 Test Categories
csharp
[TestClass]
public class SessionServiceTests
{
    [TestMethod]
    public void GetActiveSessions_ValidServer_ReturnsSessions()
    [TestMethod]
    public void SendNotification_InvalidSession_ReturnsFalse()
    [TestMethod]
    public async Task ExecuteCountdown_Cancelled_StopsGracefully()
}
11.2 Mock Objects
csharp
public class MockSessionService : ISessionService
{
    private List<SessionInfo> _mockSessions;
    
    public Task<List<SessionInfo>> GetActiveSessionsAsync(string server)
    {
        return Task.FromResult(_mockSessions);
    }
}
12. Performance Optimization
12.1 Critical Paths
Session enumeration: Cache results, batch operations

UI updates: Use BeginInvoke, throttle updates

File I/O: Async file operations

Memory: Dispose resources properly

12.2 Monitoring Points
Session query response time

UI thread responsiveness

Memory usage during execution

Network latency impact

13. Migration Notes from PowerShell
13.1 Key Differences
PowerShell Feature	C# Equivalent
query user	WMI or Terminal Services API
msg command	NetSend API or custom implementation
logoff command	WTSDisconnectSession
Write-Progress	ProgressBar + BackgroundWorker
Start-Sleep	Task.Delay or Thread.Sleep
13.2 Porting Strategy
Direct port: Keep logic identical, change syntax

Native rewrite: Use .NET APIs instead of process calls

Hybrid approach: Use PowerShell engine for complex scripts

14. Sample Code Generation Prompt for Gemini
text
Please generate a C# Windows Forms application based on this blueprint.

Requirements:
1. Create MainForm with:
   - Server input (TextBox)
   - Timer input (NumericUpDown, 60-86400 range)
   - Interval input (NumericUpDown, 10-3600 range)
   - Start/Stop/Clear buttons
   - RichTextBox output console
   
2. Implement SessionService using WTSApi32.dll:
   - WTSEnumerateSessions for session listing
   - WTSSendMessage for notifications
   - WTSDisconnectSession for termination
   
3. Add async execution with cancellation support
4. Include JSON settings persistence
5. Add progress reporting during countdown

Please output complete, compilable code files.
15. Success Criteria
15.1 Functional
All PowerShell features replicated

No external PowerShell dependencies

Settings persist between runs

Proper error messages

15.2 Non-Functional
Startup time < 2 seconds

Memory usage < 100MB

Responsive during execution

Handles 100+ sessions

15.3 User Experience
Intuitive interface

Clear status feedback

Keyboard shortcuts

Help tooltips

