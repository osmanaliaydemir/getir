# Code Review Guidelines

## 🎯 **Code Review Süreci**

### **1. Review Zorunluluğu**
- Tüm PR'lar en az 1 reviewer tarafından onaylanmalı
- Critical değişiklikler için 2+ reviewer gerekli
- Admin/Lead developer onayı gereken durumlar:
  - Security değişiklikleri
  - Database schema değişiklikleri
  - Breaking changes
  - Performance critical değişiklikler

### **2. Review Checklist**

#### **🔍 Genel Kontroller**
- [ ] Kod SOLID prensiplerine uygun mu?
- [ ] DRY (Don't Repeat Yourself) prensibi uygulanmış mı?
- [ ] Naming conventions doğru mu?
- [ ] Code formatting tutarlı mı?
- [ ] Gereksiz kod var mı?

#### **🛡️ Security Kontrolleri**
- [ ] SQL injection koruması var mı?
- [ ] XSS koruması var mı?
- [ ] Authentication/Authorization doğru mu?
- [ ] Sensitive data expose edilmiş mi?
- [ ] Input validation yapılmış mı?

#### **⚡ Performance Kontrolleri**
- [ ] N+1 query problemi var mı?
- [ ] Gereksiz database call'lar var mı?
- [ ] Memory leak riski var mı?
- [ ] Async/await doğru kullanılmış mı?
- [ ] Caching uygun şekilde kullanılmış mı?

#### **🧪 Test Kontrolleri**
- [ ] Unit testler yazılmış mı?
- [ ] Test coverage yeterli mi?
- [ ] Integration testler gerekli mi?
- [ ] Edge case'ler test edilmiş mi?

#### **📚 Documentation Kontrolleri**
- [ ] API documentation güncellenmiş mi?
- [ ] Code comment'leri yeterli mi?
- [ ] README güncellenmiş mi?
- [ ] Breaking changes dokümante edilmiş mi?

### **3. Review Etiketi**

#### **✅ Approved**
- Kod kalitesi yüksek
- Tüm checklist maddeleri tamamlanmış
- Testler başarılı
- Documentation güncel

#### **🔄 Changes Requested**
- Küçük değişiklikler gerekli
- Code quality iyileştirmeleri
- Test eklemeleri
- Documentation güncellemeleri

#### **❌ Rejected**
- Major architectural değişiklikler
- Security açıkları
- Performance problemleri
- Breaking changes

### **4. Review Yorumları**

#### **👍 Pozitif Yorumlar**
```markdown
✅ Great implementation!
✅ Clean and readable code
✅ Good use of design patterns
✅ Excellent error handling
```

#### **🔧 İyileştirme Önerileri**
```markdown
💡 Consider using async/await here
💡 This could be extracted to a separate method
💡 Consider adding input validation
💡 Performance could be improved with caching
```

#### **⚠️ Kritik Sorunlar**
```markdown
🚨 Security vulnerability detected
🚨 This could cause a memory leak
🚨 Breaking change - needs documentation
🚨 Performance impact - needs optimization
```

### **5. Review Timeline**

- **Initial Review**: 24 saat içinde
- **Follow-up Reviews**: 12 saat içinde
- **Critical Issues**: 4 saat içinde
- **Emergency Fixes**: 2 saat içinde

### **6. Review Tools**

#### **Otomatik Kontroller**
- SonarQube static analysis
- ESLint/TSLint code quality
- Prettier code formatting
- Husky pre-commit hooks

#### **Manuel Kontroller**
- Code logic review
- Architecture review
- Security review
- Performance review

### **7. Best Practices**

#### **Reviewer İçin**
- Constructive feedback verin
- Code'a değil, kişiye değil odaklanın
- Alternatif çözümler önerin
- Learning opportunity olarak görün

#### **Author İçin**
- Review feedback'lerini dikkatle okuyun
- Sorularınızı net bir şekilde sorun
- Defensive olmayın, öğrenmeye açık olun
- Feedback'leri implement edin

### **8. Review Metrics**

#### **Takip Edilen Metrikler**
- Review turnaround time
- Number of review cycles
- Code quality score
- Bug detection rate
- Knowledge sharing rate

#### **Hedefler**
- Review time < 24 hours
- Review cycles < 3
- Code quality score > 8/10
- Bug detection rate > 90%
- Knowledge sharing rate > 80%

---

**🎯 Bu guidelines ile code quality'mizi sürekli iyileştirebiliriz!**
