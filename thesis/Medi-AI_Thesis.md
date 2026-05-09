# Medi-AI

**DEPARTMENT OF COMPUTER ENGINEERING**
**BALOCHISTAN UNIVERSITY OF INFORMATION TECHNOLOGY, ENGINEERING, AND MANAGEMENT SCIENCES**
Spring 2026

**Medi-AI**

by
Abdur Rehman 59858
Zoha Shahid 60953
Attqa Khan 61965

Submitted to the
Department of Computer Engineering
In Partial Fulfillment of Requirements for the Degree of Bachelor of Science in Computer Engineering at Balochistan University of Information Technology, Engineering and Management Sciences
Spring 2026

---

## Undertaking

It is certified that this work titled “Medi-AI” is our own work. The work has not been presented elsewhere for assessment. Where material has been used from other sources, it has been properly acknowledged / referred to.

______________________
Abdur Rehman 59858

______________________
Zoha Shahid 60953

______________________
Attqa Khan 61965

---

## Acknowledgements

The Acknowledgment and the people to thank here…

---

## Dedication

Decide who will be the focus. Think about the people to whom you want to dedicate this work…

---

## Abstract

Finding swift healthcare support is challenging within a university environment, particularly for students residing in hostels far from clinical facilities. This thesis presents Medi-AI, a mobile-based healthcare assistant designed specifically for the BUITEMS community, including students, faculty, and staff. Medi-AI provides immediate, intelligent responses to user-reported symptoms, offers preliminary health guidance, and manages medication reminders with offline support. The system features a cross-platform mobile application developed using the Flutter framework, ensuring a seamless user experience across various devices. The backend is powered by ASP.NET Core 8.0 and integrated with a MySQL database to provide a robust, secure, and scalable infrastructure. A core component of the system is its intelligent symptom analysis module, which leverages the Gemini Large Language Model (LLM) to provide context-aware health advice based on natural language descriptions. By allowing users to describe symptoms in plain speech rather than through rigid questionnaires, Medi-AI enhances accessibility and ensures timely support. The application aims to reduce wait times for medical consultations and encourage proactive self-care. This project demonstrates the potential of integrating mobile technology and artificial intelligence to bridge the gap between students and healthcare providers in an institutional setting.

**Keywords:** Medi-AI, Symptom Analysis, Artificial Intelligence, Gemini LLM, Flutter, ASP.NET Core, Healthcare Accessibility, Mobile Health (mHealth).

---

## Table of Contents
1. [INTRODUCTION](#1-introduction)
   1.1 Background
   1.2 Problem statement
   1.3 Objective
   1.4 Scope
   1.5 Significance of the Study
   1.6 Organization of the Thesis
2. [LITERATURE REVIEW](#2-literature-review)
3. [METHODOLOGY](#3-methodology)
4. [SYSTEM IMPLEMENTATION](#4-system-implementation)
5. [RESULT AND DISCUSSION](#5-result-and-discussion)
6. [CONCLUSION AND FUTURE WORK](#6-conclusion-and-future-work)
References
APPENDIX A

---

## List of Figures
Figure 1: A typical embedded hardware architecture

---

## List of Tables
Table 1: Students Detail

---

## Chapter No. 1
### 1 INTRODUCTION

#### 1.1 Background
The advancement of digital technologies has significantly transformed the healthcare industry by improving communication, accessibility, and the efficiency of healthcare services. The integration of information and communication technologies into healthcare, commonly referred to as eHealth, has enabled healthcare providers to deliver medical services through digital platforms, mobile applications, and internet-based systems. According to World Health Organization, mobile health (mHealth) is a subset of eHealth that utilizes mobile and wireless technologies to support medical and public health practices.

The widespread adoption of smartphones and internet services has accelerated the growth of mobile healthcare applications worldwide. These applications allow users to access healthcare information, monitor medical conditions, schedule appointments, receive medication reminders, and communicate with healthcare professionals remotely. In recent years, Artificial Intelligence (AI) technologies have further enhanced digital healthcare systems by enabling intelligent symptom analysis, predictive healthcare assistance, and automated patient support services.

AI-powered healthcare applications have gained considerable attention due to their ability to provide preliminary medical guidance based on user-reported symptoms. These systems assist healthcare providers by reducing workload, improving response time, and enhancing patient engagement. In educational environments, such as universities, mobile healthcare systems are particularly beneficial as they provide students with immediate support, especially during hours when on-campus clinics may be closed.

At Balochistan University of Information Technology, Engineering and Management Sciences, students, faculty members, and hostel residents often face difficulties in obtaining timely medical consultation due to limited clinic operating hours, communication barriers, and delays in appointment management. These challenges create a need for an accessible and efficient healthcare support system capable of providing immediate medical guidance and streamlined communication with healthcare professionals.

To address these challenges, this project proposes Medi-AI. The proposed system is a mobile healthcare platform designed specifically for the BUITEMS community. The application integrates AI-based symptom analysis, appointment scheduling, medication reminders, and doctor consultation services into a centralized system. The primary objective of the system is to improve healthcare accessibility, enhance communication between patients and doctors, and support efficient healthcare management within the university environment.

#### 1.2 Problem statement
Access to timely healthcare services remains a significant challenge for students, faculty members, and hostel residents at BUITEMS. The university clinic may not always be available during emergencies or outside regular operating hours, making it difficult for individuals to obtain immediate medical guidance. Additionally, the absence of a centralized digital healthcare system creates communication gaps between patients and healthcare providers, resulting in delays in appointment scheduling and medical consultation.
Students residing in hostels are particularly affected due to limited transportation options and reduced access to nearby healthcare facilities. Furthermore, the lack of an organized medication reminder system may lead to missed doses and poor medication adherence.
Existing healthcare applications often focus on general healthcare services and are not specifically designed for institutional healthcare environments such as universities. Many systems also require continuous internet connectivity and lack offline support for essential healthcare features.
Therefore, there is a need for an intelligent and user-friendly mobile healthcare system that provides AI-powered symptom assessment, facilitates appointment management, supports communication with university doctors, and offers offline medication reminders tailored specifically for the BUITEMS community.

#### 1.3 Objectives
The primary objective of the Medi-AI system is to develop a mobile-based healthcare guidance and appointment management platform for the BUITEMS community.
The specific objectives of the project are as follows:
• To develop an AI-powered symptom analysis system capable of providing preliminary health guidance based on user-input symptoms.
• To design an appointment management system that allows students and faculty members to request medical consultations with university doctors.
• To provide priority healthcare support features for faculty members.
• To develop a doctor and administrator portal for managing appointments, patient records, and clinic schedules.
• To implement an offline medication reminder system that helps users maintain medication adherence without requiring internet connectivity.
• To improve accessibility and efficiency of healthcare services within the university environment.

#### 1.4 Scope of the Project
##### 1.4.1 Project Coverage
The Medi-AI system focuses on developing a mobile healthcare application specifically designed for students, faculty members, and hostel residents of Balochistan University of Information Technology, Engineering and Management Sciences. The application provides healthcare support services including symptom assessment, appointment scheduling, medication reminders, and communication with healthcare professionals.
The system is designed to improve healthcare accessibility within the university environment by enabling users to access healthcare assistance through their mobile devices. The platform also aims to reduce delays in medical consultation and improve communication between patients and doctors.

##### 1.4.2 System Boundaries
The proposed system will operate only within the BUITEMS healthcare environment and associated university clinics. The application will not integrate with external hospitals, national healthcare databases, or third-party medical systems.
The AI symptom analysis feature will provide preliminary health suggestions and recommendations based on user input; however, it will not replace professional medical diagnosis or treatment provided by qualified healthcare professionals.
The application will primarily target Android devices due to the widespread use of Android smartphones among students and faculty members.

##### 1.4.3 Technical Components of the System
The Medi-AI system is composed of several integrated technical modules designed to support digital healthcare services within the university environment. The major system components are described as follows:
• AI-Based Symptom Analysis Module: This module utilizes Artificial Intelligence algorithms to analyze user-provided symptoms and generate preliminary healthcare recommendations along with possible medical conditions.
• Appointment and Consultation Management Module: This component enables users to request medical consultations, schedule appointments with university healthcare professionals, and manage appointment records through a centralized digital scheduling system.
• Medication Reminder Module: The system includes an offline medication reminder mechanism that uses local device notifications to alert users about scheduled medication timings without requiring continuous internet access.
• Doctor and Administrative Management Portal: This module allows doctors and administrative staff to manage patient requests, monitor appointment schedules, access consultation records, and organize healthcare-related operations efficiently.
• Authentication and Role-Based Access Control Module: This component manages user authentication and authorization by providing secure access control for different user roles including students, faculty members, doctors, and administrators.
• Database Management System: The system utilizes a centralized database for storing user information, appointment records, medication schedules, consultation history, and healthcare-related data securely.

##### 1.4.4 Limitations and Constraints
The Medi-AI system has several limitations and constraints that must be considered during implementation and usage.
• The AI symptom analysis feature provides preliminary health guidance and should not be considered a substitute for professional medical diagnosis.
• Internet connectivity is required for appointment scheduling, doctor communication, and AI-based services.
• The medication reminder feature is the only component designed to function offline.
• The accuracy of AI-generated recommendations depends on the accuracy and completeness of user-provided symptom information.
• The initial version of the system will not support emergency healthcare services or video consultation features.
• The application will primarily support Android devices during the initial deployment phase.

#### 1.5 Significance of the Study
##### 1.5.1 Benefits for Students and Hostel Residents
The proposed system provides students and hostel residents with improved access to healthcare guidance and consultation services. Users can receive preliminary symptom analysis, schedule appointments, and receive medication reminders directly through their mobile devices. This improves healthcare accessibility and encourages proactive health management among students living away from their families.

##### 1.5.2 Benefits for Faculty Members and Healthcare Staff
Faculty members benefit from priority appointment support and efficient communication with healthcare providers. Healthcare staff and doctors benefit from a centralized system that simplifies appointment management, patient record handling, and clinic scheduling processes.

##### 1.5.3 Contribution to Digital Healthcare
The Medi-AI system contributes to the growing field of digital healthcare by demonstrating the integration of Artificial Intelligence and mobile healthcare technologies within an educational institution. The project may serve as a reference model for similar healthcare systems in universities and educational organizations.

##### 1.5.4 Societal Impact
The project demonstrates how mobile healthcare technologies can improve healthcare accessibility in developing regions and educational environments. By enabling early health guidance and improving communication between patients and healthcare providers, the system can contribute to better healthcare awareness and improved community well-being.

##### 1.5.5 Stakeholders and Beneficiaries
The primary stakeholders and beneficiaries of the Medi-AI system include:
• Students and Hostel Residents: Receive healthcare guidance, appointment services, and medication reminders.
• Faculty Members: Access priority healthcare consultation services.
• Doctors and Healthcare Staff: Manage patient appointments, healthcare records, and clinic schedules efficiently.
• University Administration: Improve healthcare service management and operational efficiency within the institution.

##### 1.5.6 Future Scope
The Medi-AI system can be further enhanced in future developments by integrating additional healthcare features and advanced technologies.
Possible future enhancements include:
• Integration with external hospitals and healthcare providers.
• Video consultation and telemedicine support.
• Electronic medical record synchronization.
• Wearable health device integration.
• Advanced AI models for improved symptom analysis and predictive healthcare analytics.
• Expansion of the system to other universities and educational institutions.

#### 1.6 Organization of the Thesis
The structure of this thesis is organized as follows:
Chapter 1 – Introduction: Presents the background, problem statement, objectives, scope, significance, and overview of the proposed system.
Chapter 2 – Literature Review: Reviews existing research studies, healthcare applications, and AI-based healthcare systems related to the proposed project.
Chapter 3 – System Methodology and Design: Describes the system architecture, development methodology, technologies used, and implementation process.
Chapter 4 – System Implementation: Explains the implementation of the system modules including symptom analysis, appointment management, and doctor portal functionalities.
Chapter 5 – Testing and Evaluation: Presents system testing procedures, evaluation results, and user feedback analysis.
Chapter 6 – Conclusion and Future Recommendations: Summarizes the findings of the study and discusses recommendations for future improvements.

---

## Chapter No. 2
### LITERATURE REVIEW
The goal here is to look at new studies on artificial intelligence used in medical services, along with smartphone health apps - also checking smart AI setups that act independently. We’re diving into tools for spotting symptoms while exploring ways people find doctors easily. Another focus sits on tech that helps users remember meds. This section reviews current methods and systems by questioning how they work - and where they fall short. Findings help explain why building MediAI makes sense: a phone-powered helper for health advice and alerts made just for BUITEMS.

The literature review contributes to this project by: Backing up how realistic it is to check symptoms with AI through big language systems - using tools that understand and process words like humans do, making healthcare checks quicker while still reliable. Finding weak spots in current campus health apps - like when they crash during exams or ignore mental wellness. Explain why we should add offline alerts along with nearby clinic searches.

**Theoretical Framework:**
Agentic AI: capabilities, risks, and relevance to MediAI. Latest takes see agentic AI as advanced setups running on their own, using repeated smart guesses, mixing text, visuals, sensors - hooked up to APIs or data stores - shifting gears as needed to give tailored advice in areas like spotting issues, sorting cases, tracking progress, handling tasks.

Healthcare sees better help diagnosis, custom care tips, or smoother office tasks - yet smart AI brings tough issues. These systems often fail when facing new situations, raise privacy concerns, or clash with rules. Fitting them into doctor routines isn't easy, nor is ensuring humans stay in control. Clear reasoning matters, along with solid policies to manage risks.

Agentic AI ties into MediAI by backing up key choices in its design. First, it uses LLMs or agent-style methods to understand open-ended symptoms better, giving advice that fits the situation. Instead of just linking parts together, this approach builds a clearer picture from patient input. Second, it splits smart, adaptive thinking - handled by the LLM - from fixed actions like pulling data via tools such as Google Maps or clinic lists. By keeping these apart, errors drop when sharing booking details or contacts. This setup boosts trust without losing flexibility. A recent study on next-gen agentic systems highlights how this shift can reshape health tech [1] [Next-generation agentic AI for transforming healthcare - ScienceDirect] The literature points out that showing uncertainty or confidence levels helps - plus it’s smart to involve people for trickier situations. MediAI follows this idea by offering advice instead of firm answers. It also sends users to real doctors when needed.

**Review of Existing Research**
This review pulls together those studies, spots main ideas, shows what’s strong and weak using but also reveals where MediAI steps in thanks to missing pieces found earlier.

1. **Agentic AI in Healthcare**
New research sees agentic AI as advanced tech that works on its own, uses repeated smart guesses, takes in different kinds of data like words, pictures, or sensor signals, links up with tools such as APIs or databases, adjusts choices based on situations - helping guide decisions in patient checks, sorting cases, tracking health status, and improving daily operations [1].
Strengths: Allows adaptable thinking that fits the situation - using different approaches when needed. Helps tailor care plans while streamlining office tasks.
Weaknesses / Limitations: Models might struggle when faced with data unlike what they saw before. Mixing into daily medical routines isn't straightforward - each step brings new hurdles. Folks still need to keep an eye on things - clarity matters, also solid rules help guide how it’s used.

2. **AI Symptom Checkers and Chatbots**
Evidence suggests LLMs or newer machine learning setups might help with diagnosis and sorting cases - when tasks are narrow. How well they work depends on the job, plus results can slip if used beyond their training scope. Tools combining specific features - like image readers or stored medical facts - tend to do better when handling mixed-type data. Still, risks like skewed outputs, made-up info, and patient safety issues haven't gone away.

3. **Hybrid Mobile Health Apps: Smart Doctor Case Study**
The Smart Doctor undergrad project shows a phone-based setup with two pieces: one’s a chatbot asking about symptoms (called Smiley Bot), while the other pulls data from records using optical character recognition [2].

**Technologies and Tools**
Smart Doctor (Chatbot + EHR Prototype): Practical Lessons for MediAI. The "Smart Doctor" college project shows a mobile setup with two pieces: A chatbot that asks about symptoms - called Smiley Bot - to help you figure out what to do next, guiding your first steps instead of a doctor's visit. An OCR-powered EHR system grabs data from paper docs using Google Mobile Vision - keeps copies safe with a backup option if needed.

---

## Chapter No. 3
### METHODOLOGY

#### Overview
This part shows how we built Medi AI - a smart helper for checking symptoms, suggesting treatments, maybe setting up doctor visits. Its goal? To walk through each step, layout choices, tech picked during making it.
The system helps students and faculty get medical support by checking symptoms through AI, while also linking them to open doctors for advice.
The project uses a loose, step-by-step approach shaped by Agile ideas - so changes come naturally; progress keeps moving yet shifts fit smoothly throughout each phase.

#### Problem Statement
Plenty of learners or regular folks struggle to spot health problems - then connect with qualified physicians fast. MediAI came about to fix this, offering a smart tool that checks your symptoms while hinting at likely remedies; it also sets up chats with licensed medics. This way works by building the system bit by bit - shaping, creating, checking, then improving it - to fit actual medical demands without waste or delay.

#### Methodological Approach
The approach uses an Agile Model - a step-by-step way of working that’s adaptable, letting teams test often, get reactions, then tweak things as they go through each phase.
Every sprint tackle one module at a time - shaping it, building it, then fine-tuning, so things stay flexible and run smoothly.
The approach includes these stages:
1. **Requirement Analysis:** Picked what students, doctors, and staff needed - both how things should work and how they’d use them.
2. **System Design:** Set up the structure of the system, designed how data would be stored, and also mapped out steps users take through the app.
3. **Implementation:** The backend services were developed using ASP.NET Core 8.0, providing a robust RESTful API. The cross-platform mobile application was built using the Flutter framework. Data persistence is managed through a MySQL database, interfaced via Entity Framework Core. The system integrates the Gemini Large Language Model (LLM) to perform intelligent symptom analysis and provide healthcare guidance.
4. **Testing & Evaluation:** Tested features like logging in or signing up, plus entering symptoms - checked how accurate the replies were.
5. **Deployment:** Kept on-site while being tested. Set up ready for later use on BUITEMS’ local system or online servers.

#### Tools and Technologies
The development of the Medi-AI system utilized a modern technology stack to ensure performance, scalability, and cross-platform compatibility:

1. **Backend Framework (ASP.NET Core 8.0):** Chosen for its high performance and robust security features, ASP.NET Core handles the server-side logic, user authentication (JWT), and API orchestration.
2. **Database Management (MySQL):** A centralized MySQL database stores critical information, including user profiles, appointment records, medication schedules, and audit logs. Entity Framework Core (EF Core) is used as the Object-Relational Mapper (ORM).
3. **Mobile Framework (Flutter):** Flutter enables the development of a high-fidelity, natively compiled application for both Android and iOS from a single codebase.
4. **Artificial Intelligence (Gemini LLM):** The Gemini API is integrated to provide advanced natural language processing for symptom assessment and health tips.
5. **Development Environment:** Visual Studio and Android Studio were used for backend and frontend development, respectively.
6. **Version Control (GitHub):** Used for collaborative development and code management.

---

## Chapter No. 4
### SYSTEM IMPLEMENTATION

#### 4.1 Backend Architecture
The backend is built using ASP.NET Core 8.0, following a Clean Architecture approach with Controllers, Services, and Models.
- **Controllers:** Handle HTTP requests and define API endpoints (e.g., `AuthController`, `AiController`, `AppointmentsController`).
- **Services:** Contain business logic, such as `GeminiAiService` for AI interaction and `EmailService` for OTP verification.
- **Models:** Define the database schema and Entity Framework Core entities.

#### 4.2 AI Integration (Symptom Analysis)
The system utilizes the Gemini API (`gemini-1.5-flash`) to process natural language descriptions of symptoms.
- **Prompt Engineering:** A specialized prompt is used to ensure the AI returns responses in a structured JSON format, including preliminary conditions, severity levels, and recommended actions.
- **Safety Measures:** The system is designed to provide guidance only and includes clear disclaimers that it does not replace professional medical diagnosis.

#### 4.3 Database Schema
The MySQL database consists of several interconnected tables:
- `Users`: Stores account details and roles (Student, Faculty, Doctor, Admin).
- `Doctors`: Contains professional details, specializations, and ratings.
- `Appointments`: Manages bookings between students/faculty and doctors.
- `SymptomChecks`: Records AI analysis history for users.
- `MedicineReminders`: Stores medication schedules for offline/online alerts.

#### 4.4 API Endpoints
Key API endpoints include:
- `POST /api/auth/register`: User registration with OTP.
- `POST /api/ai/analyze`: AI-powered symptom analysis.
- `POST /api/appointments`: Booking medical consultations.
- `POST /api/reminders/sync`: Synchronizing medication reminders.

---

## Chapter No. 5
### RESULT AND DISCUSSION
Provide an overview of the chapter, emphasizing the presentation and discussion of the results.

#### 5.1 Results
Present the results of your experiments or investigations in a clear and organized manner.

##### 5.1.1 Tables
Table 1: Students Detail
S.No | Name | CMS | Department | GPA
---|---|---|---|---
1 | Maqsood | 3776 | CE | 3.2
2 | Waqas | 3456 | CE | 3.1
3 | Yasir | 3467 | CE | 3.4
4 | Basit | 6544 | CE | 3.2

##### 5.1.2 Figures
Figure 1: A typical embedded hardware architecture

#### 5.2 Discussion
Interpret the results and analyze their significance in relation to the research objectives.

#### 5.3 Comparison with Previous Studies
Compare your results with those of previous studies or relevant benchmarks.

#### 5.4 Limitations and Validity
Address the limitations of your study, including any constraints or factors that may have influenced the results.

---

## Chapter No. 6
### CONCLUSION AND FUTURE WORK

#### 6.1 Conclusion
Summarize the main findings and outcomes of your project.

#### 6.2 Future Work
Identify areas for further research or improvement.

---

## References
[1] Tang, Shelden, D. R and C. Pardis, "A review of building information modeling (BIM) and the internet of things (IoT) devices integration: Present status and future trends," Automation in Construction, vol. 101, no. Elsevier, pp. 127-139, 2019.
[2] R. Weatherall, "Writing the doctoral thesis differently," Management Learning, vol. 50, no. SAGE Publications Sage UK: London, England, pp. 100-113, 2019.

---

## APPENDIX A
### CODE
```latex
\ProvideDocumentCommand{\autodot}{}{}
\ProvideDocumentCommand{\mdtChapapp}{}{}
\ProvideDocumentCommand{\chapteralign}{}{\raggedright}
\ProvideDocumentCommand{\chapterfont}{}{\Huge\bfseries}
\ProvideDocumentCommand{\chapterprefixfont}{}{\LARGE\bfseries}
```
