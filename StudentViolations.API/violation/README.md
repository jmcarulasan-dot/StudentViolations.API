# 🎓 Student Violation Management System

A comprehensive Flutter application for managing student violations in educational institutions with role-based access for Guard, Student, SAO, and Guidance Office personnel.

## 🚀 Features

### 🔐 Multi-Role Authentication
- **Guard**: Records violations at school gate
- **Student**: Views violation history and status
- **SAO**: Manages violations and escalates cases
- **Guidance**: Handles counseling and rehabilitation

### 📋 Violation Types
- ❌ No School ID
- ❌ No/Incomplete Uniform
- ❌ Visible Piercing
- ❌ Colored/Dyed Hair

### ⚡ Automated Escalation System
1. **1st Offense**: Warning issued
2. **2nd Offense**: Parent notification
3. **3rd Offense**: Referral to SAO
4. **Continued**: Guidance intervention

## 🛠️ Tech Stack

- **Flutter**: Cross-platform mobile development
- **SQLite**: Local database storage
- **Provider**: State management
- **Material Design**: Modern UI components

## 📱 Installation

### Prerequisites
- Flutter SDK (>=3.10.7)
- Dart SDK
- Android Studio / VS Code with Flutter extensions

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd violation
   ```

2. **Install dependencies**
   ```bash
   flutter pub get
   ```

3. **Run the application**
   ```bash
   flutter run
   ```

## 🔑 Demo Credentials

| Role | Username | Password |
|------|----------|----------|
| Guard | guard | guard123 |
| Student | student | student123 |
| SAO | sao | sao123 |
| Guidance | guidance | guidance123 |

## 📱 App Screenshots & Features

### Login Screen
- Role selection with visual indicators
- Secure authentication system
- Demo credentials displayed for testing

### Guard Dashboard
- Real-time violation recording
- Student search and selection
- Violation type categorization
- Recent violations history

### Student Dashboard
- Personal violation history
- Current status indicators
- Violation statistics by type
- Compliance requirements display

### SAO Dashboard
- Overview of all violations
- Case filtering and management
- Parent contact confirmation
- Referral to Guidance Office

### Guidance Dashboard
- Referred cases management
- Counseling session scheduling
- Rehabilitation action planning
- Student status tracking

## 🔄 Workflow Process

```
Student Arrives → Guard Inspection → Violation Check
                                    ↓
                             No Violation → Enter Class
                                    ↓
                            With Violation → Record
                                    ↓
                         1st Offense → Warning
                         2nd Offense → Parent Notified  
                         3rd Offense → SAO Referral
                         Continued → Guidance → Action
```

## 📊 Database Schema

### Users Table
- id, username, password, name, role, gradeSection, contactNumber

### Violations Table
- id, studentId, type, date, remarks, status, offenseCount, reportedBy

## 🎨 UI/UX Features

- **Modern Material Design**: Clean, intuitive interface
- **Color-Coded Status**: Visual indicators for violation severity
- **Responsive Layout**: Works on all screen sizes
- **Real-time Updates**: Instant status changes across roles

## 🔧 Configuration

### Database Setup
The app uses SQLite with automatic initialization and seed data:
- Default users for each role
- Sample violation types
- Pre-configured escalation rules

### Customization Options
- Add new violation types in `models/violation.dart`
- Modify escalation logic in `providers/violation_provider.dart`
- Update UI themes in `main.dart`

## 🚀 Deployment

### Android
```bash
flutter build apk --release
```

### iOS
```bash
flutter build ios --release
```

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Contact the development team

## 🔄 Future Enhancements

- [ ] Push notifications for parents
- [ ] Barcode/QR code scanning for IDs
- [ ] Analytics and reporting dashboard
- [ ] Multi-language support
- [ ] Cloud database integration
- [ ] Photo evidence capture
- [ ] Email/SMS automation

---

**Built with ❤️ using Flutter**
