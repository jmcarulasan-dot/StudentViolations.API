enum ViolationType {
  noId,
  noUniform,
  piercing,
  coloredHair,
}

enum ViolationStatus {
  pending,
  warning,
  parentNotified,
  referredToSAO,
  referredToGuidance,
  disciplinaryAction,
  cleared,
}

class Violation {
  final String id;
  final String studentId;
  final ViolationType type;
  final DateTime date;
  final String? remarks;
  final ViolationStatus status;
  final int offenseCount;
  final String? reportedBy;

  Violation({
    required this.id,
    required this.studentId,
    required this.type,
    required this.date,
    this.remarks,
    required this.status,
    required this.offenseCount,
    this.reportedBy,
  });

  Map<String, dynamic> toMap() {
    return {
      'id': id,
      'studentId': studentId,
      'type': type.name,
      'date': date.toIso8601String(),
      'remarks': remarks,
      'status': status.name,
      'offenseCount': offenseCount,
      'reportedBy': reportedBy,
    };
  }

  factory Violation.fromMap(Map<String, dynamic> map) {
    return Violation(
      id: map['id'],
      studentId: map['studentId'],
      type: ViolationType.values.firstWhere((t) => t.name == map['type']),
      date: DateTime.parse(map['date']),
      remarks: map['remarks'],
      status: ViolationStatus.values.firstWhere((s) => s.name == map['status']),
      offenseCount: map['offenseCount'],
      reportedBy: map['reportedBy'],
    );
  }

  String get violationDescription {
    switch (type) {
      case ViolationType.noId:
        return 'No School ID';
      case ViolationType.noUniform:
        return 'No/Incomplete Uniform';
      case ViolationType.piercing:
        return 'Visible Piercing';
      case ViolationType.coloredHair:
        return 'Colored/Dyed Hair';
    }
  }

  String get statusDescription {
    switch (status) {
      case ViolationStatus.pending:
        return 'Pending';
      case ViolationStatus.warning:
        return 'Warning Issued';
      case ViolationStatus.parentNotified:
        return 'Parent Notified';
      case ViolationStatus.referredToSAO:
        return 'Referred to SAO';
      case ViolationStatus.referredToGuidance:
        return 'Referred to Guidance';
      case ViolationStatus.disciplinaryAction:
        return 'Disciplinary Action';
      case ViolationStatus.cleared:
        return 'Cleared';
    }
  }
}
