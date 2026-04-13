# Briota CDSS: Presentation Storyboard 🎤

This storyboard provides a step-by-step clinical walkthrough for your live Viva demonstration.

---

## Scene 1: The "Clinical Command Center" (Dashboard)
**Goal**: Demonstrate Role-Based Access and the professional "Hardened" interface.

1.  **Action**: Log in as `dev@briota.com` / `Password123!`.
2.  **Narration**: *"The platform is now fully RBAC-enabled. As a System Admin, I can see the global clinical performance cards and patient throughput stats."*
3.  **Visual**: Highlight the clinical stats cards and the tailored navigation bar.

---

## Scene 2: High-Scale Clinical Search (Performance)
**Goal**: Prove the system can handle massive datasets (23,000+ records) without delay.

1.  **Action**: Navigate to the **Patient Directory**.
2.  **Narration**: *"We are currently managing a repository of over 23,000 medical records. Previous iterations struggled with timeouts, but through custom SQL indexing, we've achieved near-instant responsiveness."*
3.  **Action**: Search for any patient (e.g., "ketki").
4.  **Proof**: Highlight that the results appear in **under 2 seconds**.

---

## Scene 3: The Concurrency Laboratory (Safety) 🧪
**Goal**: Demonstrate the Optimistic Concurrency Control (OCC) safety logic.

### The Setup
*   Open **two browser tabs** (Tab A and Tab B) for the **same patient record** (e.g., ID: 1).

### Step 1: The Success Case (Tab A)
*   **Action**: In Tab A, change a symptom (e.g., *"Patient Stable"*) and click **Save**.
*   **Result**: The save succeeds, and the version number is rotated in the background.

### Step 2: The Blocked Case (Tab B)
*   **Action**: In Tab B, try to change the same symptom (e.g., *"CRITICAL ALERT"*) and click **Save**.
*   **Proof**: The system will **block** the save and display the red clinical alert:  
    > *"The record you attempted to edit was modified by another user after you fetched it."*

---

## Scene 4: Conclusion (The "Hardened" CDSS)
**Goal**: Summarize the transformation.

*   **Narration**: *"We have moved from a fragile, timeout-bound legacy system to a hardened, secure, and performant platform ready for clinical deployment."*

---
_Storyboard curated by Caravaggio (Presenter) & Paige (Writer)_
