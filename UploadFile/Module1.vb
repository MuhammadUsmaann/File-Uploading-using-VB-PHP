Imports System.Net
Imports System.Text
Imports System.IO
Imports System.Collections.Specialized
Imports Newtonsoft.Json.Linq
Imports System.Net.Http

Module Module1

    Sub Main()

        Dim filepath As String = "path to file"
        Dim url As String = "www.xyz.com/rest/test"
        Dim nvc As NameValueCollection = New NameValueCollection()
        nvc.Add("param1", "1")
        nvc.Add("param2", "2")
        HttpUploadFile(url, filepath, "file", "application/pdf", nvc)
        Console.ReadKey()

    End Sub

    Private Sub HttpUploadFile( _
        ByVal uri As String, _
        ByVal filePath As String, _
        ByVal fileParameterName As String, _
        ByVal contentType As String, _
        ByVal otherParameters As Specialized.NameValueCollection)

        Dim boundary As String = "---------------------------" & DateTime.Now.Ticks.ToString("x")
        Dim newLine As String = System.Environment.NewLine
        Dim boundaryBytes As Byte() = Text.Encoding.ASCII.GetBytes(newLine & "--" & boundary & newLine)
        Dim request As Net.HttpWebRequest = Net.WebRequest.Create(uri)

        request.ContentType = "multipart/form-data; boundary=" & boundary
        request.Method = "POST"
        request.KeepAlive = True
        request.Credentials = Net.CredentialCache.DefaultCredentials

        Using requestStream As IO.Stream = request.GetRequestStream()

            Dim formDataTemplate As String = "Content-Disposition: form-data; name=""{0}""{1}{1}{2}"

            For Each key As String In otherParameters.Keys

                requestStream.Write(boundaryBytes, 0, boundaryBytes.Length)
                Dim formItem As String = String.Format(formDataTemplate, key, newLine, otherParameters(key))
                Dim formItemBytes As Byte() = Text.Encoding.UTF8.GetBytes(formItem)
                requestStream.Write(formItemBytes, 0, formItemBytes.Length)

            Next key

            requestStream.Write(boundaryBytes, 0, boundaryBytes.Length)

            Dim headerTemplate As String = "Content-Disposition: form-data; name=""{0}""; filename=""{1}""{2}Content-Type: {3}{2}{2}"
            Dim header As String = String.Format(headerTemplate, fileParameterName, filePath, newLine, contentType)
            Dim headerBytes As Byte() = Text.Encoding.UTF8.GetBytes(header)
            requestStream.Write(headerBytes, 0, headerBytes.Length)

            Using fileStream As New IO.FileStream(filePath, IO.FileMode.Open, IO.FileAccess.Read)

                Dim buffer(4096) As Byte
                Dim bytesRead As Int32 = fileStream.Read(buffer, 0, buffer.Length)

                Do While (bytesRead > 0)

                    requestStream.Write(buffer, 0, bytesRead)
                    bytesRead = fileStream.Read(buffer, 0, buffer.Length)

                Loop

            End Using

            Dim trailer As Byte() = Text.Encoding.ASCII.GetBytes(newLine & "--" + boundary + "--" & newLine)
            requestStream.Write(trailer, 0, trailer.Length)

        End Using

        Dim response As Net.WebResponse = Nothing

        Try

            response = request.GetResponse()

            Using responseStream As IO.Stream = response.GetResponseStream()

                Using responseReader As New IO.StreamReader(responseStream)

                    Dim responseText = responseReader.ReadToEnd()
                    Diagnostics.Debug.Write(responseText)

                End Using

            End Using

        Catch exception As Net.WebException

            response = exception.Response

            If (response IsNot Nothing) Then

                Using reader As New IO.StreamReader(response.GetResponseStream())

                    Dim responseText = reader.ReadToEnd()
                    Diagnostics.Debug.Write(responseText)

                End Using

                response.Close()

            End If

        Finally
            request = Nothing
        End Try
    End Sub
End Module
