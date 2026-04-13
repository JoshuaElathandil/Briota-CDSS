# Briota CDSS: Hardened Documentation Hub 🏗️

Welcome to the technical documentation for the **Briota Clinical Decision Support System (CDSS)**. This repository has been transformed from a legacy codebase into a high-performance, secure, and clinical-grade command center.

## 🌟 Quick Links

### 📋 [The Hardening Dossier](HARDENING_REPORT.md)
Discover the "Before vs. After" of our optimization journey. Includes real-world performance benchmarks ($50s \to < 2s$ search speed) and SQL robustness metrics.

### 📐 [Architecture Blueprint](architecture-blueprint.md)
A deep dive into the patterns that protect our data, including **Optimistic Concurrency Control** and our global **Multi-tenant Security** filters.

### 🧪 [Laboratory Technical Guide](LABORATORY_GUIDE.md)
Detailed operational guide for the Search, Concurrency, and Stability labs used in the Viva demonstration.

### 🎭 [Presentation Storyboard](PRESENTATION_STORYBOARD.md)
Step-by-step guidance for your live Viva demonstration, specifically the "Concurrency Laboratory" sequence.

---

## 🏛️ System Pillars

### 1. Performance & Scalability
The system is optimized to handle **23,000+ patient records** with ease. We utilize primary key sorting, custom SQL indexing, and 180s command timeouts to ensure zero clinical downtime.

### 2. Clinical Integrity
Our **"Load-and-Patch"** architecture ensures that clinical data is never accidentally overwritten. The integrated **Concurrency Laboratory** provides a deterministic way to prove this safety logic in a live demo.

### 3. Security Isolation
Data isolation is enforced at the core database level via **Global Query Filters**. This ensures that clinicians only see data relevant to their specific medical center, preventing any chance of IDOR or BOLA vulnerabilities.

---
_Documentation curated by Paige (Technical Writer) & Winston (Architect)_
