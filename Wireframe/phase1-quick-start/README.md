# Phase 1: Quick Start Wireframes
*3-Question Retirement Calculator - Simple & Professional*

## Overview
This phase captures users with the minimum viable questions to show impressive projections and hook them into the full experience.

## Wireframes

### 01-quick-start-questions.drawio
**3 Essential Questions - 30 Second Input**

**Design Elements:**
- Clean, single-question focus
- Progress dots (● ○ ○) showing step 1 of 3
- Large, touch-friendly input fields
- Professional blue accent color (#007acc)
- Trust signals ("🔒 Takes 30 seconds • Secure & Private")

**Questions Asked:**
1. **Current Age** (32) - Auto-focused, number keypad
2. **Current Savings** ($85,000) - Currency formatting
3. **Monthly Savings** ($1,200) - Currency formatting

**UX Strategy:**
- First field auto-focused with blue border
- One question feels active, others neutral
- Strong CTA: "Show My Retirement Projection 🚀"
- No overwhelming financial jargon

### 02-quick-results.drawio
**Impressive Results - Hook & Tease**

**Key Elements:**
- **Big Win**: "$1.47M" in large, prominent text
- **Encouragement**: "You're off to a fantastic start! 🌟"
- **Quick Summary Card**: Simple breakdown without complexity
- **Phase 2 Teaser**: "Answer 3 more questions to unlock..."

**Psychological Triggers:**
- Large, impressive number creates excitement
- Positive reinforcement builds confidence  
- FOMO elements (unlock employer matching, etc.)
- Clear next action with benefits listed

## Design Principles

### Professional & Trustworthy
- Clean typography and spacing
- Conservative color palette (blues, grays)
- Consistent button styling
- Security messaging

### Mobile-First
- Single-column layout
- Large touch targets (45px+ height)
- Readable font sizes (16px+ for inputs)
- Thumb-friendly button placement

### Conversion-Focused
- Minimal cognitive load (one question focus)
- Strong visual hierarchy 
- Clear progress indication
- Compelling next steps

## Technical Notes

### Default Assumptions (Phase 1)
- **Retirement Age**: 65 (standard assumption)
- **Investment Return**: 7% annually (conservative market average)
- **Contribution Growth**: 0% (conservative, no salary increases)
- **Inflation**: Not factored in Phase 1 (keeps numbers simple)

### Calculation Formula
```
Future Value = Current Savings × (1.07)^years + 
               Monthly × [((1.07)^years - 1) / 0.07] × 12
```

### Next Phase Transition
Users who complete Phase 1 see teaser for:
- Employer matching calculations
- Retirement income planning
- Personalized recommendations
- More accurate projections

## Success Metrics to Track
- Completion rate of 3 questions
- Time to complete Phase 1
- Click-through rate to Phase 2
- User excitement/satisfaction (if surveyed)

---
*Goal: Get users excited about their retirement potential in under 60 seconds*